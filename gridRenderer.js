window.gridRenderer = (() => {
    const _cache = {};
    const _rafState = {};

    const _playback = {};
    const CELL = 64;

    function setData(cacheId, map, gridMap, path, config) {
        const offscreen = buildOffscreenMap(map);
        _cache[cacheId] = { map, gridMap, path, config, offscreen };
    }

    function draw(canvas, cacheId, scale, offsetX, offsetY, currentFrame) {
        const state = _rafState[cacheId] ?? (_rafState[cacheId] = {});

        state.pending = { canvas, cacheId, scale, offsetX, offsetY, currentFrame };

        if (!state.rafId) {
            state.rafId = requestAnimationFrame(() => {
                state.rafId = null;
                const p = state.pending;
                if (p) _drawNow(p.canvas, p.cacheId, p.scale, p.offsetX, p.offsetY, p.currentFrame);
            });
        }
    }

    function startPlayback(canvas, cacheId, scale, offsetX, offsetY, startFrame, msPerFrame, dotNetRef) {
        stopPlayback(cacheId); 

        const cached = _cache[cacheId];
        if (!cached) return;

        const maxFrame = getMaxFrame(cached.path);
        let frame = startFrame;

        _playback[cacheId] = setInterval(() => {
            frame++;

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

    function _drawNow(canvas, cacheId, scale, offsetX, offsetY, currentFrame) {
        const cached = _cache[cacheId];
        if (!canvas || !cached) return;

        const { map, gridMap, path, config, offscreen } = cached;
        const ctx = canvas.getContext('2d');

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

        ctx.drawImage(offscreen, 0, 0);

        if (config?.showChunks)
            drawChunks(ctx, map, config.chunkSize, CELL);

        if (config?.showConnections)
            drawConnections(ctx, gridMap, CELL);

        if (config?.showGates)
            drawGates(ctx, gridMap, CELL);

        if (true)
            drawPath(ctx, path, CELL, currentFrame);

        ctx.restore();
    }

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

    function getMaxFrame(path) {
        return path?.animationSteps?.length > 0
            ? path.animationSteps.length - 1
            : 0;
    }

    return { setData, draw, startPlayback, stopPlayback };

})();