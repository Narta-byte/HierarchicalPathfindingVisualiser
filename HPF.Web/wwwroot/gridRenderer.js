window.gridRenderer = {

    draw: function (
        canvas,
        rows,
        cols,
        mapString,
        scale,
        offsetX,
        offsetY
    ) {

        if (!canvas)
            return;

        const ctx = canvas.getContext("2d");

        const CELL_SIZE = 16;

        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        ctx.clearRect(0, 0, canvas.width, canvas.height);

        ctx.save();

        ctx.translate(offsetX, offsetY);
        ctx.scale(scale, scale);

        for (let y = 0; y < rows; y++) {

            for (let x = 0; x < cols; x++) {

                const i = y * cols + x;
                const c = mapString[i];

                switch (c) {

                    case '#':
                        ctx.fillStyle = "#222";
                        break;

                    case 'S':
                        ctx.fillStyle = "#00aa00";
                        break;

                    case 'G':
                        ctx.fillStyle = "#cc0000";
                        break;

                    default:
                        ctx.fillStyle = "#d4d0c8";
                        break;
                }

                ctx.fillRect(
                    x * CELL_SIZE,
                    y * CELL_SIZE,
                    CELL_SIZE,
                    CELL_SIZE
                );

                ctx.strokeStyle = "#999";

                ctx.strokeRect(
                    x * CELL_SIZE,
                    y * CELL_SIZE,
                    CELL_SIZE,
                    CELL_SIZE
                );
            }
        }

        ctx.restore();
    }
};