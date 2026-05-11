using BenchmarkDotNet.Disassemblers;
using System.ComponentModel;
using System.Diagnostics;
using static HPF.model.PMap;

namespace HPF.model {
    public class PMap(int n, int m) {
        public enum Celltype {
            Empty,
            Wall,
            Start,
            Goal
        }

        int n = n;
        int m = m;
        public Vector2? start;
        public Vector2? goal;
        public Celltype[,] cells = new Celltype[n, m];

        public int N => n;
        public int M => m;

        public Vector2? Start() => start;
        public Vector2? Goal => goal;

        public bool IsWall(int row, int col)
            => cells[row, col] == Celltype.Wall;

        public PMap MapFromStr(int n, int m, string mapStr) {
            if (mapStr.Length != n * m)
                throw new ArgumentException(
                    $"mapStr length must be {n * m}, but was {mapStr.Length}");
            this.n = n;
            this.m = m;
            cells = new Celltype[n, m];
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    char c = mapStr[i * m + j];
                    switch (c) {
                        case 'S':
                            cells[i, j] = Celltype.Start;
                            start = new Vector2(Row: i, Col: j);
                            break;
                        case 'G':
                            cells[i, j] = Celltype.Goal;
                            goal = new Vector2(Row: i, Col: j);
                            break;
                        case '#':
                            cells[i, j] = Celltype.Wall;
                            break;
                        default:
                            cells[i, j] = Celltype.Empty;
                            break;
                    }
                }
            }
            return this;
        }

        public Dictionary<Vector2, Node> ToNodes(Celltype[,] map) {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);

            var nodes = new Dictionary<Vector2, Node>();

            // Create one node per walkable cell
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    if (map[r, c] == Celltype.Wall)
                        continue;

                    var pos = new Vector2(r, c);
                    nodes[pos] = new Node(pos, new List<Node>());
                }
            }

            // Connect neighbors
            var dirs = new (int dr, int dc)[]
            {
                (-1, 0),
                (1, 0),
                (0, -1),
                (0, 1)
            };

            foreach (var node in nodes.Values) {
                foreach (var (dr, dc) in dirs) {
                    int nr = node.Pos.Row + dr;
                    int nc = node.Pos.Col + dc;

                    if (nr < 0 || nc < 0 || nr >= rows || nc >= cols)
                        continue;

                    if (map[nr, nc] == Celltype.Wall)
                        continue;

                    var neighborPos = new Vector2(nr, nc);

                    if (nodes.TryGetValue(neighborPos, out var neighbor)) {
                        if (!node.Connections.Contains(neighbor))
                            node.Connections.Add(neighbor);
                    }
                }
            }

            return nodes;
        }
        public FinalPath Run(IAlgo algo) {
            var nodes = ToNodes(cells);   // build the full graph once
            var startNode = nodes[start];
            var goalNode = nodes[goal];

            FinalPath p = Run(algo);
            return algo.FindGoal(startNode, goalNode);
        }


    }
}