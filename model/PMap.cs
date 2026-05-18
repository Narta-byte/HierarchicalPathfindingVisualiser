using BenchmarkDotNet.Disassemblers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
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
        private int[,] components = new int[n, m];

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
                            components[i, j] = i * m + j;
                            break;
                        case 'G':
                            cells[i, j] = Celltype.Goal;
                            goal = new Vector2(Row: i, Col: j);
                            components[i, j] = i * m + j;

                            break;
                        case '#':
                            cells[i, j] = Celltype.Wall;
                            components[i, j] = -1;

                            break;
                        default:
                            cells[i, j] = Celltype.Empty;
                            components[i, j] = i * m + j;

                            break;
                    }
                }
            }
            return this;
        }
        public PMap InitComponents() {
            CreateComponents(components);
            return this;
        }

        protected int[,] CreateComponents(int[,] domain) {
            // Build a union-find parent array
            Dictionary<int, int> parent = new();

            int Find(int x) {
                if (!parent.ContainsKey(x)) parent[x] = x;
                if (parent[x] != x) parent[x] = Find(parent[x]); // path compression
                return parent[x];
            }

            void Union(int a, int b) {
                int rootA = Find(a);
                int rootB = Find(b);
                if (rootA != rootB) {
                    parent[rootB] = rootA; // merge
                }
            }

            // First pass: union adjacent cells
            int dim0 = domain.GetLength(0);
            int dim1 = domain.GetLength(1);
            for (int row = 0; row < dim0; row++) {
                for (int col = 0; col < dim1; col++) {
                    int current = domain[row, col];
                    if (current == -1) continue;

                    // Check right neighbor
                    if (col + 1 < dim1) {
                        int right = domain[row, col + 1];
                        if (right != -1) Union(current, right);
                    }

                    // Check down neighbor
                    if (row + 1 < dim0) {
                        int down = domain[row + 1, col];
                        if (down != -1) Union(current, down);
                    }
                }
            }
            
            // Second pass: update all cells to their root component
            for (int row = 0; row < dim0; row++) {
                for (int col = 0; col < dim1; col++) {
                    if (domain[row, col] != -1) {
                        domain[row, col] = Find(domain[row, col]);
                    }
                }
            }
            
                return domain;
        }
        


        protected bool ConnectedNodes(Node n1, Node n2) 
            => components[n1.Pos.Row, n1.Pos.Col] == components[n2.Pos.Row, n2.Pos.Col];
        protected bool Connected((int row, int col) n1, (int row, int col) n2)
            => components[n1.row, n1.col] == components[n2.row, n2.col];

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