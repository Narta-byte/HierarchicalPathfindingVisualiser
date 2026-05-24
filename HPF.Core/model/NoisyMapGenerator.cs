using System;
using System.Text;
using HPF.model;
public static class NoisyMapGenerator {
    public static string Generate(int n, int m, double wallChance = 0.25, int? seed = null) {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var map = new char[n, m];

        // Fill with empty cells or walls
        for (int r = 0; r < n; r++) {
            for (int c = 0; c < m; c++) {
                map[r, c] = rng.NextDouble() < wallChance ? '#' : '.';
            }
        }

        // Place start and goal
        var start = RandomEmptyCell(map, rng);
        map[start.Row, start.Col] = 'S';

        Vector2 goal;
        do {
            goal = RandomEmptyCell(map, rng);
        } while (goal == start);

        map[goal.Row, goal.Col] = 'G';

        // Flatten to string
        var sb = new StringBuilder(n * m);
        for (int r = 0; r < n; r++) {
            for (int c = 0; c < m; c++)
                sb.Append(map[r, c]);
        }

        return sb.ToString();
    }

    private static Vector2 RandomEmptyCell(char[,] map, Random rng) {
        int n = map.GetLength(0);
        int m = map.GetLength(1);

        while (true) {
            int r = rng.Next(n);
            int c = rng.Next(m);

            if (map[r, c] == '.')
                return new Vector2(r, c);
        }
    }
    public static void PrintAsRows(string mapStr, int n, int m) {
        for (int r = 0; r < n; r++) {
            Console.WriteLine(mapStr.Substring(r * m, m));
        }
    }

}
