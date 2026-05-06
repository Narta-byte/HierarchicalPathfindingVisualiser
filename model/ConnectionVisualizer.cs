using System;
using System.Collections.Generic;
using System.Linq;
using HPF.model;

public static class ConnectionVisualizer {
    public static void PrintConnections(GridMap gridMap) {
        int n = gridMap.N;
        int m = gridMap.M;
        int gridSize = gridMap.GridSize;

        var gates = gridMap.GetAllGates().ToList();
        var gateSet = gates.Select(g => g.Pos).ToHashSet();

        // Collect all cells that belong to connections
        var connectionCells = new HashSet<Vector2>();
        var seenEdges = new HashSet<string>();

        foreach (var gate in gates) {
            foreach (var other in gate.Connections) {
                // avoid drawing same edge twice
                string key = MakeEdgeKey(gate.Pos, other.Pos);
                if (!seenEdges.Add(key))
                    continue;

                foreach (var cell in GetLine(gate.Pos, other.Pos))
                    connectionCells.Add(cell);
            }
        }

        Console.CursorVisible = false;

        for (int r = 0; r < n; r++) {
            for (int c = 0; c < m; c++) {
                var pos = new Vector2(r, c);

                bool onV = (c % gridSize == 0);
                bool onH = (r % gridSize == 0);

                // connection lines first
                if (connectionCells.Contains(pos) && !gateSet.Contains(pos)) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write('*');
                    continue;
                }

                // gates
                if (gateSet.Contains(pos)) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('G');
                    continue;
                }

                // chunk boundaries
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

                // chunk interior coloring
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
        Console.WriteLine("Legend: G = gate, * = connection, +|-| = chunk boundaries");
        Console.CursorVisible = true;
    }

    private static string MakeEdgeKey(Vector2 a, Vector2 b) {
        // order-independent key
        if (a.Row < b.Row || (a.Row == b.Row && a.Col <= b.Col))
            return $"{a.Row},{a.Col}-{b.Row},{b.Col}";

        return $"{b.Row},{b.Col}-{a.Row},{a.Col}";
    }

    private static IEnumerable<Vector2> GetLine(Vector2 a, Vector2 b) {
        // simple Manhattan-style line drawing
        int r = a.Row;
        int c = a.Col;

        while (r != b.Row || c != b.Col) {
            if (r != b.Row) {
                r += Math.Sign(b.Row - r);
            } else if (c != b.Col) {
                c += Math.Sign(b.Col - c);
            }

            yield return new Vector2(r, c);
        }
    }
}
