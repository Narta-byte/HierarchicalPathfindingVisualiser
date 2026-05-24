window.gridRenderer = {

    pending: false,

    draw: function (canvas, rows, cols, scale, offsetX, offsetY) {

        if (this.pending)
            return;

        this.pending = true;

        requestAnimationFrame(() => {

            this.pending = false;

            const ctx = canvas.getContext("2d");

            if (!ctx)
                return;

            const rect = canvas.getBoundingClientRect();

            if (canvas.width !== rect.width)
                canvas.width = rect.width;

            if (canvas.height !== rect.height)
                canvas.height = rect.height;

            ctx.clearRect(0, 0, canvas.width, canvas.height);

            const cellSize = 24;

            ctx.save();

            ctx.translate(offsetX, offsetY);
            ctx.scale(scale, scale);

            ctx.fillStyle = "#e0e0e0";
            ctx.strokeStyle = "#8a8a8a";

            const viewLeft   = -offsetX / scale;
            const viewTop    = -offsetY / scale;

            const viewRight  = viewLeft + canvas.width / scale;
            const viewBottom = viewTop + canvas.height / scale;

            const startX = Math.max(0, Math.floor(viewLeft / cellSize));
            const startY = Math.max(0, Math.floor(viewTop / cellSize));

            const endX = Math.min(cols, Math.ceil(viewRight / cellSize));
            const endY = Math.min(rows, Math.ceil(viewBottom / cellSize));

            for (let y = startY; y < endY; y++) {

                for (let x = startX; x < endX; x++) {

                    const px = x * cellSize;
                    const py = y * cellSize;

                    ctx.fillRect(px, py, cellSize, cellSize);
                    ctx.strokeRect(px, py, cellSize, cellSize);
                }
            }

            ctx.restore();
        });
    }
};