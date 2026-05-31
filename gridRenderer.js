/**
 * gridRenderer.js — optimised canvas renderer for HPF grid maps.
 *
 * Key optimisations vs. the original:
 *   1. Static data (map, gates, connections) is cached in JS and only sent
 *      once from Blazor, so the hot draw path only transfers 5 small scalars.
 *   2. The map tile layer is pre-rendered to an offscreen canvas and blit'd
 *      with drawImage() — no per-cell fillRect on every pan/zoom.
 *   3. All draw calls are guarded by requestAnimationFrame so we never draw
 *      more than once per display refresh, even if Blazor fires many events.
 *   4. Playback is driven entirely by a JS setInterval, removing the Blazor
 *      Task.Delay loop and all per-frame .NET→JS interop overhead during play.
 */

window.gridRenderer = (() => {

    // ─── per-instance cache keyed by cacheId ────────────────────────────────
    const _cache = {};

    // ─── per-instance rAF state ──────────────────────────────────────────────
    const _rafState = {};

    // ─── per-instance playback state ────────────────────────────────────────
    const _playback = {};

    const CELL = 16;

    // ────────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Called once (or whenever Map/Path change) to push heavy data into JS.
     * Builds the offscreen tile canvas here so draw() never rebuilds it.
     */
    function setData(cacheId, map, gridMap, path, config) {
        const offscreen = buildOffscreenMap(map);
        _cache[cacheId] = { map, gridMap, path, config, offscreen };
    }

    /**
     * Hot draw call — only view-state scalars arrive from Blazor.
     * Schedules a real draw via rAF, discarding redundant calls in the same
     * frame.
     */
    function draw(canvas, cacheId, scale, offsetX, offsetY, currentFrame) {
        const state = _rafState[cacheId] ?? (_rafState[cacheId] = {});

        // Always update the latest requested view state
        state.pending = { canvas, cacheId, scale, offsetX, offsetY, currentFrame };

        if (!state.rafId) {
            state.rafId = requestAnimationFrame(() => {
                state.rafId = null;
                const p = state.pending;
                if (p) _drawNow(p.canvas, p.cacheId, p.scale, p.offsetX, p.offsetY, p.currentFrame);
            });
        }
    }

    /**
     * Start JS-driven playback.
     * @param dotNetRef  Blazor DotNetObjectReference — we call OnFrameTick /
     *                   OnPlaybackEnded on it so the slider stays in sync.
     */
    function startPlayback(canvas, cacheId, scale, offsetX, offsetY, startFrame, msPerFrame, dotNetRef) {
        stopPlayback(cacheId); // cancel any existing interval

        const cached = _cache[cacheId];
        if (!cached) return;

        const maxFrame = getMaxFrame(cached.path);
        let frame = startFrame;

        _playback[cacheId] = setInterval(() => {
            frame++;

            // Sync slider every 3 frames to reduce .NET calls without
            // making the slider look janky.
            if (frame % 3 === 0) {
                dotNetRef.invokeMethodAsync('OnFrameTick', frame);
            }

            _drawNow(canvas, cacheId, scale, offsetX, offsetY, frame);

            if (frame >= maxFrame) {
                stopPlayback(cacheId);
                dotNetRef.invokeMethodAsync('OnPlaybackEnded', frame);
            }
        }, msPerFrame);
    }

    function stopPlayback(cacheId) {
        if (_playback[cacheId]) {
            clearInterval(_playback[cacheId]);
            delete _playback[cacheId];
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Internal draw
    // ────────────────────────────────────────────────────────────────────────

    function _drawNow(canvas, cacheId, scale, offsetX, offsetY, currentFrame) {
        const cached = _cache[cacheId];
        if (!canvas || !cached) return;

        const { map, gridMap, path, config, offscreen } = cached;
        const ctx = canvas.getContext('2d');

        // Size the canvas to its wrapper, never to the window.
        // The wrapper must have a fixed size via CSS (position:absolute fill, or
        // explicit height) — that is what prevents the page scrollbar.
        const wrapper = canvas.parentElement;
        const W = wrapper ? wrapper.clientWidth  : 800;
        const H = wrapper ? wrapper.clientHeight : 600;
        if (canvas.width !== W || canvas.height !== H) {
            canvas.width  = W;
            canvas.height = H;
        }

        ctx.clearRect(0, 0, W, H);
        ctx.save();
        ctx.translate(offsetX, offsetY);
        ctx.scale(scale, scale);

        // ── layer 1: static map tiles (offscreen blit — very cheap) ──
        ctx.drawImage(offscreen, 0, 0);

        // ── layer 2: chunk grid ──
        if (config?.showChunks)
            drawChunks(ctx, map, config.chunkSize, CELL);

        // ── layer 3: gate connections ──
        if (config?.showConnections)
            drawConnections(ctx, gridMap, CELL);

        // ── layer 4: gates ──
        if (config?.showGates)
            drawGates(ctx, gridMap, CELL);

        // ── layer 5: path animation ──
        if (true)
            drawPath(ctx, path, CELL, currentFrame);

        ctx.restore();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Offscreen tile canvas — built once in setData()
    // ────────────────────────────────────────────────────────────────────────

    function buildOffscreenMap(map) {
        const oc  = document.createElement('canvas');
        oc.width  = map.cols * CELL;
        oc.height = map.rows * CELL;
        const ctx = oc.getContext('2d');

        const str = map.mapString;
        for (let r = 0; r < map.rows; r++) {
            for (let c = 0; c < map.cols; c++) {
                const ch = str[r * map.cols + c];
                if      (ch === '#') ctx.fillStyle = '#202020';
                else if (ch === 'S') ctx.fillStyle = '#00aa00';
                else if (ch === 'G') ctx.fillStyle = '#aa0000';
                else                 ctx.fillStyle = '#d0d0d0';

                ctx.fillRect(c * CELL, r * CELL, CELL, CELL);
            }
        }
        return oc;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Layer draw functions (unchanged logic, moved inside module)
    // ────────────────────────────────────────────────────────────────────────

    function drawChunks(ctx, map, chunkSize, cellSize) {
        ctx.strokeStyle = '#0088ff';
        ctx.lineWidth   = 2;

        for (let r = 0; r <= map.rows; r += chunkSize) {
            ctx.beginPath();
            ctx.moveTo(0,                r * cellSize);
            ctx.lineTo(map.cols * cellSize, r * cellSize);
            ctx.stroke();
        }
        for (let c = 0; c <= map.cols; c += chunkSize) {
            ctx.beginPath();
            ctx.moveTo(c * cellSize, 0);
            ctx.lineTo(c * cellSize, map.rows * cellSize);
            ctx.stroke();
        }
    }

    function drawGates(ctx, gridMap, cellSize) {
        const gates = gridMap?.allGates;
        if (!Array.isArray(gates)) return;

        ctx.fillStyle = '#ba4e23';
        for (const gate of gates) {
            const x = gate?.pos?.col;
            const y = gate?.pos?.row;
            if (x == null || y == null) continue;

            ctx.beginPath();
            ctx.arc(
                x * cellSize + cellSize / 2,
                y * cellSize + cellSize / 2,
                cellSize * 0.3,
                0, Math.PI * 2
            );
            ctx.fill();
        }
    }

    function drawConnections(ctx, gridMap, cellSize) {
        const gates = gridMap?.allGates;
        if (!Array.isArray(gates)) return;

        ctx.strokeStyle = '#f4a629';
        ctx.lineWidth   = 3;

        for (const gate of gates) {
            if (!gate?.pos || !Array.isArray(gate.connections)) continue;

            for (const conn of gate.connections) {
                if (conn?.row == null || conn?.col == null) continue;

                ctx.beginPath();
                ctx.moveTo(gate.pos.col * cellSize + cellSize / 2,
                           gate.pos.row * cellSize + cellSize / 2);
                ctx.lineTo(conn.col      * cellSize + cellSize / 2,
                           conn.row      * cellSize + cellSize / 2);
                ctx.stroke();
            }
        }
    }

    function drawPath(ctx, path, cellSize, currentFrame) {
        if (!path?.animationSteps) return;

        // Batch visited cells then path cells to minimise fillStyle switches
        const visited = [];
        const pathCells = [];

        for (let i = 0; i <= currentFrame; i++) {
            const step = path.animationSteps[i];
            if (!step?.pos) continue;
            if (step.isPath)    pathCells.push(step.pos);
            else if (step.isVisited) visited.push(step.pos);
        }

        ctx.fillStyle = '#7f9928';
        for (const pos of visited) {
            ctx.fillRect(pos.col * cellSize + 3, pos.row * cellSize + 3,
                         cellSize - 6, cellSize - 6);
        }

        ctx.fillStyle = '#cd0d0d';
        for (const pos of pathCells) {
            ctx.fillRect(pos.col * cellSize + 3, pos.row * cellSize + 3,
                         cellSize - 6, cellSize - 6);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Utility
    // ────────────────────────────────────────────────────────────────────────

    function getMaxFrame(path) {
        return path?.animationSteps?.length > 0
            ? path.animationSteps.length - 1
            : 0;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Exports
    // ────────────────────────────────────────────────────────────────────────

    return { setData, draw, startPlayback, stopPlayback };

})();