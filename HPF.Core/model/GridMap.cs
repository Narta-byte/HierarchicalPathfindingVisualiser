
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace HPF.model {
    //public record Gate(Node Node, List<Gate> Connections);
    public record Gate(Node MapNode, Node GateNode);
    public class Chunk(Vector2 corner1, Vector2 corner2) {
        public Vector2 Corner1 { get; init; } = corner1;
        public Vector2 Corner2 { get; init; } = corner2;
        public List<Gate> Gates { get; init; } = new();
        public Node?[,] Nodes { get; init; } = new Node[corner2.Row - corner1.Row, corner2.Col - corner1.Col];

        //public Dictionary<Vector2, Node> NodesByPos { get; init; } = new();

        public bool Contains(Vector2 pos) =>
            pos.Row >= Corner1.Row && pos.Row < Corner2.Row &&
            pos.Col >= Corner1.Col && pos.Col < Corner2.Col;
    }


    public class GridMap : PMap {
        public bool isUsingOneGatePerEdge = false;
        public GridMap SetIsUsingOneGatePerEdge(bool toggle) {
            isUsingOneGatePerEdge = toggle;
            return this;
        }
        int gridSize = 4;

        public int GridSize => gridSize;
    public List<Node> AllGates =>
        GetAllGates().ToList();
        Chunk[,] ChunksV2 = new Chunk[0, 0];
        public GridMap(int n, int m, int gridSize) : base(n, m) {
            this.gridSize = gridSize;

            int ChunkV2Rows = (N + this.gridSize - 1) / this.gridSize;
            int ChunkV2Cols = (M + this.gridSize - 1) / this.gridSize;

            ChunksV2 = new Chunk[ChunkV2Rows, ChunkV2Cols];
        }

        public GridMap InitChunks() {
            int chunkRows = (N + gridSize - 1) / gridSize;
            int chunkCols = (M + gridSize - 1) / gridSize;

            ChunksV2 = new Chunk[chunkRows, chunkCols];

            for (int cr = 0; cr < chunkRows; cr++) {
                for (int cc = 0; cc < chunkCols; cc++) {
                    int r1 = cr * gridSize;
                    int c1 = cc * gridSize;

                    int r2 = Math.Min(r1 + gridSize, N);
                    int c2 = Math.Min(c1 + gridSize, M);

                    int rowCount = r2 - r1;
                    int colCount = c2 - c1;

                    var chunk = new Chunk(
                        new Vector2(Row: r1, Col: c1),
                        new Vector2(Row: r2, Col: c2)
                    );

                    // initialize nodes
                    for (int i = r1; i < r2; i++) {
                        for (int j = c1; j < c2; j++) {
                            chunk.Nodes[i - r1, j - c1] =
                                cells[i, j] == Celltype.Wall
                                    ? null
                                    : new Node(new Vector2(i, j), new List<Node>());
                        }
                    }

                    // connect nodes
                    for (int i = 0; i < rowCount; i++) {
                        for (int j = 0; j < colCount; j++) {
                            Node? n = chunk.Nodes[i, j];
                            if (n == null) continue;

                            // down
                            if (j < colCount - 1) {
                                Node? down = chunk.Nodes[i, j + 1];
                                if (down != null) {
                                    n.Connections.Add(down);
                                    down.Connections.Add(n);
                                }
                            }
                            // right
                            if (i < rowCount - 1) {
                                Node? right = chunk.Nodes[i + 1, j];
                                if (right != null) {
                                    n.Connections.Add(right);
                                    right.Connections.Add(n);
                                }
                            }
                        }
                    }

                    ChunksV2[cr, cc] = chunk;
                }
            }
            return this;
        }

        public GridMap InitGatesV2() {
            int chunkRows = ChunksV2.GetLength(0);
            int chunkCols = ChunksV2.GetLength(1);

            for (int r = 0; r < chunkRows; r++) {
                for (int c = 0; c < chunkCols; c++) {
                    Chunk chunk = ChunksV2[r, c];
                    int rowCount = chunk.Nodes.GetLength(0);
                    int colCount = chunk.Nodes.GetLength(1);

                    // Connect chunk below
                    if (r + 1 < chunkRows) {
                        Chunk down = ChunksV2[r + 1, c];
                        int downColCount = down.Nodes.GetLength(1);
                        int limit = Math.Min(colCount, downColCount);

                        for (int col = 0; col < limit; col++) {
                            var bottomRow = chunk.Nodes[rowCount - 1, col];
                            var topRow = down.Nodes[0, col];

                            if (bottomRow != null && topRow != null) {
                                Gate bottomGate = GetOrCreateGate(chunk, bottomRow);
                                Gate topGate = GetOrCreateGate(down, topRow);

                                if (!bottomGate.GateNode.Connections.Contains(topGate.GateNode)){
                                    bottomGate.GateNode.Connections.Add(topGate.GateNode);
                                    bottomGate.MapNode.Connections.Add(topGate.MapNode);
                                }

                                if (!topGate.GateNode.Connections.Contains(bottomGate.GateNode)){
                                    topGate.GateNode.Connections.Add(bottomGate.GateNode);
                                    topGate.MapNode.Connections.Add(bottomGate.MapNode);
                                }

                                if (isUsingOneGatePerEdge)
                                    break;
                            }
                        }
                    }

                    // Connect chunk to the right
                    if (c + 1 < chunkCols) {
                        Chunk right = ChunksV2[r, c + 1];
                        int rightRowCount = right.Nodes.GetLength(0);
                        int limit = Math.Min(rowCount, rightRowCount);

                        for (int row = 0; row < limit; row++) {
                            var rightEdge = chunk.Nodes[row, colCount - 1];
                            var leftEdge = right.Nodes[row, 0];

                            if (rightEdge != null && leftEdge != null) {
                                Gate rightGate = GetOrCreateGate(chunk, rightEdge);
                                Gate leftGate = GetOrCreateGate(right, leftEdge);

                                if (!rightGate.GateNode.Connections.Contains(leftGate.GateNode)){
                                    rightGate.GateNode.Connections.Add(leftGate.GateNode);
                                    rightGate.MapNode.Connections.Add(leftGate.MapNode);
                                }

                                if (!leftGate.GateNode.Connections.Contains(rightGate.GateNode)) {
                                    leftGate.GateNode.Connections.Add(rightGate.GateNode);
                                    leftGate.MapNode.Connections.Add(rightGate.MapNode); 
                                }


                                if (isUsingOneGatePerEdge)
                                    break;
                            }
                        }
                    }
                }
            }
            var a = ChunksV2;
            return this;
        }

        private Gate GetOrCreateGate(Chunk chunk, Node node) {
            var existing = chunk.Gates.FirstOrDefault(g => g.MapNode.Pos == node.Pos);
            if (existing != null)
                return existing;

            var gate = new Gate(node, new Node(node.Pos, []));
            chunk.Gates.Add(gate);
            return gate;
        }

        public Chunk GetChunkV2(Vector2 coord) => ChunksV2[coord.Row / gridSize, coord.Col / gridSize];
        protected bool ConnectedRestricted(int[,] domain, Chunk chunk, Node n1, Node n2) {
            int r1 = n1.Pos.Row - chunk.Corner1.Row;
            int c1 = n1.Pos.Col - chunk.Corner1.Col;
            int r2 = n2.Pos.Row - chunk.Corner1.Row;
            int c2 = n2.Pos.Col - chunk.Corner1.Col;

            if (r1 < 0 || c1 < 0 || r1 >= domain.GetLength(0) || c1 >= domain.GetLength(1)) return false;
            if (r2 < 0 || c2 < 0 || r2 >= domain.GetLength(0) || c2 >= domain.GetLength(1)) return false;
            if (domain[r1, c1] == -1 || domain[r2, c2] == -1) return false;

            return domain[r1, c1] == domain[r2, c2];
        }
        public IEnumerable<Node> GetAllGates() {
            var seen = new HashSet<Gate>();
            for (int i = 0; i < ChunksV2.GetLength(0); i++) {
                for (int j = 0; j < ChunksV2.GetLength(1); j++) {
                    foreach (var gate in ChunksV2[i, j].Gates)
                        if (seen.Add(gate))
                            yield return gate.MapNode;
                }
            }
        }

        internal GridMap InitConnections() {
            for (int i = 0; i < ChunksV2.GetLength(0); i++) {
                for (int j = 0; j < ChunksV2.GetLength(1); j++) {
                    var chunk = ChunksV2[i, j];
                    int[,] chunkConnectedComponents = GenerateChunkConnectedComponent(chunk);
                    var gates = chunk.Gates;

                    if (gates.Count < 2)
                        continue;


                    var nodes = chunk.Nodes;


                    for (int u = 0; u < gates.Count; u++) {
                        Gate startGate = gates[u];
                        //var localRow = startGate.Pos.Row - ChunkV2.Corner1.Row;
                        //var localCol = startGate.Pos.Col - ChunkV2.Corner1.Col;
                        //var startNode = nodes[localRow, localCol];
                        Node startNode = startGate.MapNode;
                        for (int v = u + 1; v < gates.Count; v++) {
                            Gate goalGate = gates[v];
                            //var localGoalRow = goalGate.Pos.Row - ChunkV2.Corner1.Row;
                            //var localGoalCol = goalGate.Pos.Col - ChunkV2.Corner1.Col;
                            //var goalNode = nodes[localGoalRow, localGoalCol];
                            var goalNode = goalGate.MapNode;

                            //FinalPath res = algo.FindGoal(startNode, goalNode); // this can be replaced later by using connected components
                            //bool isConnected = ConnectedNodes(startNode, goalNode);
                            bool isConnected = ConnectedRestricted(chunkConnectedComponents,chunk, startNode, goalNode);

                            if (isConnected) {
                                if (!gates[u].GateNode.Connections.Contains(gates[v].GateNode)){
                                    gates[u].GateNode.Connections.Add(gates[v].GateNode);
                                    //gates[u].MapNode.Connections.Add(gates[v].MapNode);
                                }

                                if (!gates[v].GateNode.Connections.Contains(gates[u].GateNode)){
                                    gates[v].GateNode.Connections.Add(gates[u].GateNode);
                                    //gates[v].MapNode.Connections.Add(gates[u].MapNode);
                                }
                            }
                        }
                    }
                }
            }
            var c = ChunksV2;
            return this;
        }

        private int[,] GenerateChunkConnectedComponent(Chunk chunkV2) {
            int dim0 = chunkV2.Corner2.Row - chunkV2.Corner1.Row;
            int dim1 = chunkV2.Corner2.Col - chunkV2.Corner1.Col;
            int[,] domain = new int[dim0, dim1];
            for (int i = 0; i<dim0;i++) {
                for (int j = 0; j < dim1; j++) {
                    domain[i, j] = chunkV2.Nodes[i, j] == null
                                    ? -1              // wall
                                    : i * dim1 + j;  // unique id
                }
            }
            return CreateComponents(domain);
        }

        public GridMap ConnectStartGate() {
            if (start == null) {
                throw new Exception();
            }
            ConnectNodeToGates(start);
            return this;
        }
        public GridMap ConnectGoalGate() {
            if (goal == null) {
                throw new Exception();
            }
            ConnectNodeToGates(goal);
            return this;
        }
        private void ConnectNodeToGates(Vector2 nodePos) {

            Chunk chunk = GetChunkV2(nodePos);
            int[,] connectedComponetChunk = GenerateChunkConnectedComponent(chunk);
            var gates = chunk.Gates;
            var localRow = nodePos.Row - chunk.Corner1.Row;
            var localCol = nodePos.Col - chunk.Corner1.Col;
            var node = chunk.Nodes[localRow, localCol];
            if (node == null) {
                throw new Exception();
            }
            var newGate = new Gate(node, new Node(node.Pos, []));
            chunk.Gates.Add(newGate);

            foreach (Gate gate in gates) {
                if (gate.Equals(newGate))
                    continue;
                //bool isConnected = ConnectedNodes(newGate.MapNode, gate.MapNode); // there needs to be a chunk level connected component system, otherwise if there is a wall that disconnects
                bool isConnected = ConnectedRestricted(connectedComponetChunk, chunk, newGate.MapNode, gate.MapNode); // there needs to be a chunk level connected component system, otherwise if there is a wall that disconnects
                if (isConnected) {
                    newGate.GateNode.Connections.Add(gate.GateNode);
                    gate.GateNode.Connections.Add(newGate.GateNode);
                }
            }
        }
        public FinalPath GetGridPath(IAlgo algo) {
            if (start == null || goal == null) {
                throw new Exception();
            }
            Chunk startChunkV2 = GetChunkV2(start);
            Chunk goalChunkV2 = GetChunkV2(goal);

            var startGate = startChunkV2.Gates.Find(gate => gate.MapNode.Pos == start);
            var goalGate = goalChunkV2.Gates.Find(gate => gate.MapNode.Pos == goal);
            
            if (startGate == null)
                throw new Exception("Start is null");
            if (goalGate == null)
                throw new Exception("Goal is null");
            if (!ConnectedNodes(startGate.MapNode, goalGate.MapNode))
                throw new Exception("Start and goal is to reachable to each other");
            

            FinalPath ChunkV2Path = algo.FindGoal(startGate.GateNode, goalGate.GateNode); 
            var ChunkV2Set = new HashSet<Chunk>();

            foreach (Node node in ChunkV2Path.nodes) {
                var c = GetChunkV2(node.Pos);
                ChunkV2Set.Add(c);

            }
            var a = 1;

            return algo.FindGoal(startGate.MapNode, goalGate.MapNode, (Node n) => ChunkV2Set.Contains(GetChunkV2(n.Pos)));
            //return ChunkV2Path; // temp
        }

    }

}

