
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace HPF.model {
    //public record Gate(Node Node, List<Gate> Connections);
    public record Gate(Node Node, List<Gate> Connections);
    public class ChunkV2(Vector2 corner1, Vector2 corner2) {
        public Vector2 Corner1 { get; init; } = corner1;
        public Vector2 Corner2 { get; init; } = corner2;
        public List<Gate> Gates { get; init; } = new();
        public Node?[,] Nodes { get; init; } = new Node[corner2.Row - corner1.Row, corner2.Col - corner1.Col];

        //public Dictionary<Vector2, Node> NodesByPos { get; init; } = new();

        public bool Contains(Vector2 pos) =>
            pos.Row >= Corner1.Row && pos.Row < Corner2.Row &&
            pos.Col >= Corner1.Col && pos.Col < Corner2.Col;
    }


    public class GridMapV2 : PMap {
        public bool isUsingOneGatePerEdge = false;
        public GridMapV2 SetIsUsingOneGatePerEdge(bool toggle) {
            isUsingOneGatePerEdge = toggle;
            return this;
        }
        int gridSize = 4;

        public int GridSize => gridSize;

        ChunkV2[,] ChunksV2 = new ChunkV2[0, 0];
        public GridMapV2(int n, int m, int gridSize) : base(n, m) {
            this.gridSize = gridSize;

            int ChunkV2Rows = (N + this.gridSize - 1) / this.gridSize;
            int ChunkV2Cols = (M + this.gridSize - 1) / this.gridSize;

            ChunksV2 = new ChunkV2[ChunkV2Rows, ChunkV2Cols];
        }

        public GridMapV2 InitChunks() {
            int chunkRows = (N + gridSize - 1) / gridSize;
            int chunkCols = (M + gridSize - 1) / gridSize;

            ChunksV2 = new ChunkV2[chunkRows, chunkCols];

            for (int cr = 0; cr < chunkRows; cr++) {
                for (int cc = 0; cc < chunkCols; cc++) {
                    int r1 = cr * gridSize;
                    int c1 = cc * gridSize;

                    int r2 = Math.Min(r1 + gridSize, N);
                    int c2 = Math.Min(c1 + gridSize, M);

                    int rowCount = r2 - r1;
                    int colCount = c2 - c1;

                    var chunk = new ChunkV2(
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

        public GridMapV2 InitGatesV2() {
            int chunkRows = ChunksV2.GetLength(0);
            int chunkCols = ChunksV2.GetLength(1);

            for (int r = 0; r < chunkRows; r++) {
                for (int c = 0; c < chunkCols; c++) {
                    ChunkV2 chunk = ChunksV2[r, c];
                    int rowCount = chunk.Nodes.GetLength(0);
                    int colCount = chunk.Nodes.GetLength(1);

                    // Connect chunk below
                    if (r + 1 < chunkRows) {
                        ChunkV2 down = ChunksV2[r + 1, c];
                        int downColCount = down.Nodes.GetLength(1);
                        int limit = Math.Min(colCount, downColCount);

                        for (int col = 0; col < limit; col++) {
                            var bottomRow = chunk.Nodes[rowCount - 1, col];
                            var topRow = down.Nodes[0, col];

                            if (bottomRow != null && topRow != null) {
                                Gate bottomGate = GetOrCreateGate(chunk, bottomRow);
                                Gate topGate = GetOrCreateGate(down, topRow);

                                if (!bottomGate.Connections.Contains(topGate))
                                    bottomGate.Connections.Add(topGate);

                                if (!topGate.Connections.Contains(bottomGate))
                                    topGate.Connections.Add(bottomGate);

                                if (isUsingOneGatePerEdge)
                                    break;
                            }
                        }
                    }

                    // Connect chunk to the right
                    if (c + 1 < chunkCols) {
                        ChunkV2 right = ChunksV2[r, c + 1];
                        int rightRowCount = right.Nodes.GetLength(0);
                        int limit = Math.Min(rowCount, rightRowCount);

                        for (int row = 0; row < limit; row++) {
                            var rightEdge = chunk.Nodes[row, colCount - 1];
                            var leftEdge = right.Nodes[row, 0];

                            if (rightEdge != null && leftEdge != null) {
                                Gate rightGate = GetOrCreateGate(chunk, rightEdge);
                                Gate leftGate = GetOrCreateGate(right, leftEdge);

                                if (!rightGate.Connections.Contains(leftGate))
                                    rightGate.Connections.Add(leftGate);

                                if (!leftGate.Connections.Contains(rightGate))
                                    leftGate.Connections.Add(rightGate);

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

        private Gate GetOrCreateGate(ChunkV2 chunk, Node node) {
            var existing = chunk.Gates.FirstOrDefault(g => g.Node.Pos == node.Pos);
            if (existing != null)
                return existing;

            var gate = new Gate(node, new List<Gate>());
            chunk.Gates.Add(gate);
            return gate;
        }

        public ChunkV2 GetChunkV2(Vector2 coord) => ChunksV2[coord.Row / gridSize, coord.Col / gridSize];

        public IEnumerable<Node> GetAllGates() {
            var seen = new HashSet<Gate>();
            for (int i = 0; i < ChunksV2.GetLength(0); i++) {
                for (int j = 0; j < ChunksV2.GetLength(1); j++) {
                    foreach (var gate in ChunksV2[i, j].Gates)
                        if (seen.Add(gate))
                            yield return gate.Node;
                }
            }
        }

        internal GridMapV2 InitConnections() {
            for (int i = 0; i < ChunksV2.GetLength(0); i++) {
                for (int j = 0; j < ChunksV2.GetLength(1); j++) {
                    var ChunkV2 = ChunksV2[i, j];
                    var gates = ChunkV2.Gates;

                    if (gates.Count < 2)
                        continue;


                    var nodes = ChunkV2.Nodes;


                    for (int u = 0; u < gates.Count; u++) {
                        Gate startGate = gates[u];
                        //var localRow = startGate.Pos.Row - ChunkV2.Corner1.Row;
                        //var localCol = startGate.Pos.Col - ChunkV2.Corner1.Col;
                        //var startNode = nodes[localRow, localCol];
                        Node startNode = startGate.Node;
                        for (int v = u + 1; v < gates.Count; v++) {
                            Gate goalGate = gates[v];
                            //var localGoalRow = goalGate.Pos.Row - ChunkV2.Corner1.Row;
                            //var localGoalCol = goalGate.Pos.Col - ChunkV2.Corner1.Col;
                            //var goalNode = nodes[localGoalRow, localGoalCol];
                            var goalNode = goalGate.Node;

                            //FinalPath res = algo.FindGoal(startNode, goalNode); // this can be replaced later by using connected components
                            bool isConnected = ConnectedNodes(startNode, goalNode);

                            if (isConnected) {
                                if (!gates[u].Connections.Contains(gates[v]))
                                    gates[u].Connections.Add(gates[v]);

                                if (!gates[v].Connections.Contains(gates[u]))
                                    gates[v].Connections.Add(gates[u]);
                            }
                        }
                    }
                }
            }
            var c = ChunksV2;
            return this;
        }
        //internal GridMapV2 InitComponents() {



        //    return this;
        //}


        //private Vector2 GetLocalVector2(ChunkV2 ChunkV2, Vector2 pos) {
        //    Debug.Assert(ChunkV2 != null);
        //    Debug.Assert(pos.Row >= ChunkV2.Corner1.Row && pos.Row < ChunkV2.Corner2.Row, message: $"input vector2 out of bounds row : {pos.Row}");
        //    Debug.Assert(pos.Col >= ChunkV2.Corner1.Col && pos.Col < ChunkV2.Corner2.Col);

        //    return new Vector2(pos.Row - ChunkV2.Corner1.Row, pos.Col - ChunkV2.Corner1.Col);
        //}

        //private Celltype[,] GetLocalChunkV2(ChunkV2 ChunkV2) {

        //    int rowLen = ChunkV2.Corner2.Row - ChunkV2.Corner1.Row;
        //    int colLen = ChunkV2.Corner2.Col - ChunkV2.Corner1.Col;
        //    Celltype[,] output = new Celltype[rowLen, colLen];

        //    for (int i = 0; i < rowLen; i++) {
        //        for (int j = 0; j < colLen; j++) {
        //            output[i, j] = cells[ChunkV2.Corner1.Row + i, ChunkV2.Corner1.Col + j];
        //        }
        //    }

        //    return output;
        //}
        //private Dictionary<Vector2, Node> LocalChunkV2ToNodes(ChunkV2 ChunkV2) {
        //    Celltype[,] ChunkV2Map = GetLocalChunkV2(ChunkV2);
        //    int rows = ChunkV2Map.GetLength(0);
        //    int cols = ChunkV2Map.GetLength(1);

        //    var nodes = new Dictionary<Vector2, Node>();

        //    // create walkable cell nodes
        //    for (int r = 0; r < rows; r++) {
        //        for (int c = 0; c < cols; c++) {
        //            if (ChunkV2Map[r, c] == Celltype.Wall)
        //                continue;

        //            var pos = new Vector2(r, c);
        //            nodes[pos] = new Node(pos, new List<Node>());
        //        }
        //    }

        //    // connect cell neighbors
        //    var dirs = new (int dr, int dc)[] {
        //    (-1, 0), (1, 0), (0, -1), (0, 1)
        //};

        //    foreach (var node in nodes.Values) {
        //        foreach (var (dr, dc) in dirs) {
        //            var nextPos = new Vector2(node.Pos.Row + dr, node.Pos.Col + dc);
        //            if (nodes.TryGetValue(nextPos, out var neighbor)) {
        //                if (!node.Connections.Contains(neighbor))
        //                    node.Connections.Add(neighbor);
        //            }
        //        }
        //    }

        //    // attach gates
        //    foreach (var gate in ChunkV2.Gates) {
        //        var localPos = GetLocalVector2(ChunkV2, gate.Pos);

        //        if (!nodes.TryGetValue(localPos, out var cellNode))
        //            throw new InvalidOperationException($"Gate at {gate.Pos} is not on a walkable cell.");

        //        // connect gate node to the cell node
        //        if (!gate.Connections.Contains(cellNode))
        //            gate.Connections.Add(cellNode);

        //        if (!cellNode.Connections.Contains(gate))
        //            cellNode.Connections.Add(gate);
        //    }

        //    return nodes;
        //}
        //private Dictionary<Vector2, Node> GlobalChunkV2ToNodes(ChunkV2 ChunkV2) {
        //    Celltype[,] ChunkV2Map = GetLocalChunkV2(ChunkV2);
        //    int rows = ChunkV2Map.GetLength(0);
        //    int cols = ChunkV2Map.GetLength(1);

        //    var nodes = new Dictionary<Vector2, Node>();

        //    // create walkable cell nodes with GLOBAL positions
        //    for (int r = 0; r < rows; r++) {
        //        for (int c = 0; c < cols; c++) {
        //            if (ChunkV2Map[r, c] == Celltype.Wall)
        //                continue;

        //            var globalPos = new Vector2(
        //                ChunkV2.Corner1.Row + r,
        //                ChunkV2.Corner1.Col + c
        //            );

        //            nodes[globalPos] = new Node(globalPos, new List<Node>());
        //        }
        //    }

        //    // connect cell neighbors
        //    var dirs = new (int dr, int dc)[]
        //    {
        //    (-1, 0), (1, 0), (0, -1), (0, 1)
        //    };

        //    foreach (var node in nodes.Values) {
        //        foreach (var (dr, dc) in dirs) {
        //            var nextPos = new Vector2(node.Pos.Row + dr, node.Pos.Col + dc);

        //            if (nodes.TryGetValue(nextPos, out var neighbor)) {
        //                if (!node.Connections.Contains(neighbor))
        //                    node.Connections.Add(neighbor);
        //            }
        //        }
        //    }
        //    return nodes;
        //}
        public GridMapV2 ConnectStartGate() {
            if (start == null) {
                throw new Exception();
            }
            ConnectNodeToGates(start);
            return this;
        }
        public GridMapV2 ConnectGoalGate() {
            if (goal == null) {
                throw new Exception();
            }
            ConnectNodeToGates(goal);
            return this;
        }
        private void ConnectNodeToGates(Vector2 nodePos) {

            ChunkV2 chunk = GetChunkV2(nodePos);
            var gates = chunk.Gates;
            var localRow = nodePos.Row - chunk.Corner1.Row;
            var localCol = nodePos.Col - chunk.Corner1.Col;
            var node = chunk.Nodes[localRow, localCol];
            if (node == null) {
                throw new Exception();
            }
            var newGate = new Gate(node, []);
            //chunk.Gates.Add(newNode);

            foreach (Gate gate in gates) {
                if (gate.Equals(newGate))
                    continue;
                bool isConnected = ConnectedNodes(newGate.Node, gate.Node); // there needs to be a chunk level connected component system, otherwise if there is a wall that disconnects
                if (isConnected) {
                    newGate.Connections.Add(gate);
                    gate.Connections.Add(newGate);
                }
            }
        }
        public FinalPath GetGridPath(IAlgo algo) {
            if (start == null || goal == null) {
                throw new Exception();
            }
            ChunkV2 startChunkV2 = GetChunkV2(start);
            ChunkV2 goalChunkV2 = GetChunkV2(goal);

            var startGate = startChunkV2.Gates.Find(gate => gate.Node.Pos == start);
            var goalGate = startChunkV2.Gates.Find(gate => gate.Node.Pos == goal);

            (Node startNode, Node goalNode) = GatesToNode(startGate, goalGate);


            FinalPath ChunkV2Path = algo.FindGoal(startNode, goalNode); // there needs to be some function that converts the gates graph to a node graph
                                                                        //FinalPath ChunkV2Path = new();
            var ChunkV2Set = new HashSet<ChunkV2>();

            foreach (Node node in ChunkV2Path.nodes) {
                var c = GetChunkV2(node.Pos);
                ChunkV2Set.Add(c);

            }
            var a = 1;

            //(Node startingGate, FinalPath startingPath) = GetConnectedGate(startChunkV2, start, algo); // can be replaced by connected components
            //(Node goalGate, FinalPath goalPath) = GetConnectedGate(goalChunkV2, goal, algo); // can be replaced by connected components

            //FinalPath ChunkV2Path = algo.FindGoal(startingGate, goalGate);
            //var ChunkV2Set = new HashSet<ChunkV2>();
            //Dictionary<ChunkV2, (Node, Node?)> map = [];
            //foreach (Node node in ChunkV2Path.nodes) {
            //    var c = GetChunkV2(node.Pos);
            //    ChunkV2Set.Add(c);

            //}
            //var restrictedMap = BuildRestrictedMap(ChunkV2Set);

            //return algo.FindGoal(restrictedMap[start], restrictedMap[goal]);
            return ChunkV2Path; // temp
        }

        private (Node startNode, Node goalNode) GatesToNode(Gate startGate, Gate goalGate) {
            throw new NotImplementedException();
        }

        //private Dictionary<Vector2, Node> BuildRestrictedMap(HashSet<ChunkV2> ChunkV2Set) {
        //    List<(Dictionary<Vector2, Node> NodeMap, List<Node> Gates)> disconnectedNodeMaps = [];
        //    Dictionary<Vector2, Node> mergedMap = new();

        //    // Build per-ChunkV2 node maps and merge them into one dictionary
        //    foreach (ChunkV2 ChunkV2 in ChunkV2Set) {
        //        var nodeMap = GlobalChunkV2ToNodes(ChunkV2);
        //        disconnectedNodeMaps.Add((nodeMap, ChunkV2.Gates));

        //        foreach (var kvp in nodeMap) {
        //            mergedMap[kvp.Key] = kvp.Value;
        //        }
        //    }

        //    // Connect gates between neighboring ChunkV2s
        //    for (int i = 0; i < disconnectedNodeMaps.Count; i++) {
        //        for (int j = i; j < disconnectedNodeMaps.Count; j++) {
        //            var nodemap1 = disconnectedNodeMaps[i];
        //            var nodemap2 = disconnectedNodeMaps[j];

        //            foreach (var gate1 in nodemap1.Gates) {
        //                foreach (var gate2 in nodemap2.Gates) {
        //                    var node1 = nodemap1.NodeMap[gate1.Pos];
        //                    var node2 = nodemap2.NodeMap[gate2.Pos];

        //                    if (Adjacent(gate1, gate2) &&
        //                        !node1.Connections.Contains(node2) &&
        //                        !node2.Connections.Contains(node1)) {

        //                        node1.Connections.Add(node2);
        //                        node2.Connections.Add(node1);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return mergedMap;
        //}



        //private bool Adjacent(Node nodeA, Node nodeB) {
        //    int dr = Math.Abs(nodeA.Pos.Row - nodeB.Pos.Row);
        //    int dc = Math.Abs(nodeA.Pos.Col - nodeB.Pos.Col);
        //    return dr + dc == 1;
        //}

        //private (Node Gate, FinalPath Path) GetConnectedGate(ChunkV2 ChunkV2, Vector2 pos, IAlgo algo) {
        //    List<Node> gates = ChunkV2.Gates;
        //    var map = LocalChunkV2ToNodes(ChunkV2);
        //    var localPos = GetLocalVector2(ChunkV2, pos);
        //    var startingNode = map[localPos];
        //    int pathLength = int.MaxValue;
        //    FinalPath path = new();
        //    Node? closestGate = null;
        //    foreach (Node gate in gates) {

        //        path = algo.FindGoal(startingNode, gate);
        //        if (path.path.Count <= 0)
        //            continue;
        //        if (path.path.Count < pathLength || closestGate == null) {
        //            closestGate = gate;
        //            pathLength = path.path.Count;

        //        }
        //    }
        //    if (closestGate == null || path == null)
        //        throw new Exception();

        //    return (closestGate, path);
        //}

    }

}


//Cols →        0            4            8           10
//            |------------|------------|------------|
//Rows 0      | [0,0]      g [0,1]      | [0,2]      |
//            | (0,0)      | (0,4)      | (0,8)      |
//            |    →       |    →       g    →       |
//            | (4,4)      | (4,8)      | (4,10)     |
//Rows 4      |-g----------|-g----------|------------|
//            | [1,0]      | [1,1]      | [1,2]      |
//            | (4,0)      g (4,4)      | (4,8)      |
//            |    →       |    →       |    →       |
//            | (7,4)      | (7,8)      | (7,10)     |
//Rows 7      |------g-----|------------|------------|