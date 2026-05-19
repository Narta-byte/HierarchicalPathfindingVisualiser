using HPF.model;
using System;
using System.Collections.Generic;
using System.Linq;
using static HPF.model.Gate;

public static class ChunkVisualizer {
    public static void PrintChunksWithGates(GridMap gridMap) {
        int n = gridMap.N;
        int m = gridMap.M;
        int gridSize = gridMap.GridSize;

        // Collect gate coordinates
        var gateSet = gridMap.GetAllGates()
            .Select(g => g.Pos)
            .ToHashSet();

        Console.CursorVisible = false;

        for (int r = 0; r < n; r++) {
            for (int c = 0; c < m; c++) {
                var coord = new Vector2(r, c);

                bool onV = (c % gridSize == 0);
                bool onH = (r % gridSize == 0);

                // Gate overrides boundary drawing
                if (gateSet.Contains(coord)) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('G');
                    continue;
                }

                if (onV && onH) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write('+');
                    continue;
                }

                if (onH) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write('-');
                    continue;
                }

                if (onV) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write('|');
                    continue;
                }

                // interior
                int chunkRow = r / gridSize;
                int chunkCol = c / gridSize;
                int chunkId = (chunkRow * 7 + chunkCol * 3) % 4;

                Console.ForegroundColor = chunkId switch {
                    0 => ConsoleColor.DarkCyan,
                    1 => ConsoleColor.DarkGreen,
                    2 => ConsoleColor.DarkMagenta,
                    3 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.Gray
                };

                Console.Write('.');
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        Console.ResetColor();
        Console.WriteLine("Legend: +|-| = chunk boundaries, G = gate, '.' = chunk interior");
        Console.CursorVisible = true;
    }
}
