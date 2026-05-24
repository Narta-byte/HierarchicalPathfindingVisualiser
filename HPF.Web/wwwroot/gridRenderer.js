window.gridRenderer = {

    draw: function (
        canvas,
        map,
        gridMap,
        path,
        config,
        scale,
        offsetX,
        offsetY
    ) {
        // console.log("Drawing grid...");
        // console.log("Canvas:", canvas);
        // console.log("Map:", map);
        // console.log("GridMap:", gridMap);
        // console.log("Path:", path);
        // console.log("Config:", config);
        if (!canvas || !map)
            return;

        const ctx = canvas.getContext("2d");

        const cellSize = 16;

        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        ctx.clearRect(0, 0, canvas.width, canvas.height);

        ctx.save();

        ctx.translate(offsetX, offsetY);
        ctx.scale(scale, scale);

        drawMap(ctx, map, cellSize);

        if (config?.showChunks)
            drawChunks(ctx, map, config.chunkSize, cellSize);

        if (config?.showConnections)
            drawConnections(ctx, gridMap, cellSize);

        if (config?.showGates)
            drawGates(ctx, gridMap, cellSize);

        if (config?.showPath)
            drawPath(ctx, path, cellSize);

        ctx.restore();
    }
};

function drawMap(ctx, map, cellSize) {

    const str = map.mapString;

    for (let r = 0; r < map.rows; r++) {

        for (let c = 0; c < map.cols; c++) {

            const ch = str[r * map.cols + c];

            if (ch === '#')
                ctx.fillStyle = "#202020";
            else
                ctx.fillStyle = "#d0d0d0";

            if (ch === 'S')
                ctx.fillStyle = "#00aa00";

            if (ch === 'G')
                ctx.fillStyle = "#aa0000";

            ctx.fillRect(
                c * cellSize,
                r * cellSize,
                cellSize,
                cellSize
            );
        }
    }
}

function drawChunks(ctx, map, chunkSize, cellSize) {

    ctx.strokeStyle = "#0088ff";
    ctx.lineWidth = 2;

    for (let r = 0; r <= map.rows; r += chunkSize) {

        ctx.beginPath();

        ctx.moveTo(0, r * cellSize);

        ctx.lineTo(
            map.cols * cellSize,
            r * cellSize
        );

        ctx.stroke();
    }

    for (let c = 0; c <= map.cols; c += chunkSize) {

        ctx.beginPath();

        ctx.moveTo(c * cellSize, 0);

        ctx.lineTo(
            c * cellSize,
            map.rows * cellSize
        );

        ctx.stroke();
    }
}

function drawGates(ctx, gridMap, cellSize) {
    const gates = gridMap?.allGates;
    if (!Array.isArray(gates)) return;

    ctx.fillStyle = "yellow";

    for (const gate of gates) {
        if (!gate?.pos) continue;

        const x = gate.pos.col;
        const y = gate.pos.row;

        if (x == null || y == null) continue;

        ctx.beginPath();
        ctx.arc(
            x * cellSize + cellSize / 2,
            y * cellSize + cellSize / 2,
            cellSize * 0.3,
            0,
            Math.PI * 2
        );
        ctx.fill();
    }
}

function drawPath(ctx, path, cellSize) {

    if (!path?.path)
        return;

    ctx.fillStyle = "#00ffff";

    for (const p of path.path) {

        ctx.fillRect(
            p.col * cellSize + 4,
            p.row * cellSize + 4,
            cellSize - 8,
            cellSize - 8
        );
    }
}

function drawConnections(ctx, gridMap, cellSize) {

    const gates = gridMap?.allGates;

    if (!Array.isArray(gates))
        return;

    ctx.strokeStyle = "#ff00ff";
    ctx.lineWidth = 1;

    for (const gate of gates) {

        if (!gate?.pos)
            continue;

        if (!Array.isArray(gate.connections))
            continue;

        for (const conn of gate.connections) {

            if (conn?.row == null || conn?.col == null)
                continue;

            ctx.beginPath();

            ctx.moveTo(
                gate.pos.col * cellSize + cellSize / 2,
                gate.pos.row * cellSize + cellSize / 2
            );

            ctx.lineTo(
                conn.col * cellSize + cellSize / 2,
                conn.row * cellSize + cellSize / 2
            );

            ctx.stroke();
        }
    }
}