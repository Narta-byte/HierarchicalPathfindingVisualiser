
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace HPF.model {
    public record Chunk(Vector2 Corner1, Vector2 Corner2, List<Node> Gates);
    public enum Direction { Up, Down, Left, Right }

    public class GridMap : PMap {
        public bool isUsingOneGatePerEdge = false;
        public GridMap SetIsUsingOneGatePerEdge(bool toggle) {
            isUsingOneGatePerEdge = toggle;
            return this;
        }
        int gridSize = 4;

        public int GridSize => gridSize;

        Chunk[,] chunks = new Chunk[0, 0];

        public GridMap(int n, int m, int gridSize) : base(n, m) {
            this.gridSize = gridSize;

            int chunkRows = (N + this.gridSize - 1) / this.gridSize;
            int chunkCols = (M + this.gridSize - 1) / this.gridSize;

            chunks = new Chunk[chunkRows, chunkCols];
        }

        public GridMap InitChunks() {
            int chunkRows = (N + gridSize - 1) / gridSize;
            int chunkCols = (M + gridSize - 1) / gridSize;

            chunks = new Chunk[chunkRows, chunkCols];

            for (int cr = 0; cr < chunkRows; cr++) {
                for (int cc = 0; cc < chunkCols; cc++) {
                    int r1 = cr * gridSize;
                    int c1 = cc * gridSize;

                    int r2 = Math.Min(r1 + gridSize, N);
                    int c2 = Math.Min(c1 + gridSize, M);

                    chunks[cr, cc] = new Chunk(
                        new Vector2(Row: r1, Col: c1),
                        new Vector2(Row: r2, Col: c2),
                        new List<Node>()
                    );
                }
            }
            return this;
        }

        public GridMap InitGates() {
            List<(int, int)> directions = new List<(int, int)> { (0, 1), (1, 0) };
            for (int i = 0; i < chunks.GetLength(0); i++) {
                for (int j = 0; j < chunks.GetLength(1); j++) {
                    Chunk curChunk = chunks[i, j];
                    foreach ((int, int) dir in directions) {
                        int rowdir = i + dir.Item1;
                        int coldir = j + dir.Item2;
                        if (rowdir < 0 || rowdir >= chunks.GetLength(0) || coldir < 0 || coldir >= chunks.GetLength(1))
                            continue;


                        FindGates(curChunk, chunks[rowdir, coldir], dir);
                    }
                }
            }
            return this;

        }

        private void FindGates(Chunk curChunk, Chunk chunk, (int, int) dir) {
            switch (GetDirection(dir)) {
                case Direction.Left: {
                        (Celltype[] leftcells, Vector2 leftstart, Vector2 leftend) = GetLeftEdge(curChunk);
                        (Celltype[] rightcells, Vector2 rightstart, Vector2 rightend) = GetRightEdge(chunk);

                        var minLen = Math.Min(leftcells.Length, rightcells.Length);
                        for (int i = 0; i < minLen; i++) {
                            if (leftcells[i] == Celltype.Wall || rightcells[i] == Celltype.Wall)
                                continue;

                            Node curChunkGate = new(new Vector2(leftstart.Row + i, leftstart.Col), Connections: []);
                            Node chunkGate = new(new Vector2(rightstart.Row + i, rightstart.Col), Connections: []);

                            curChunkGate.Connections.Add(chunkGate);
                            chunkGate.Connections.Add(curChunkGate);

                            curChunk.Gates.Add(curChunkGate);
                            chunk.Gates.Add(chunkGate);
                            if (isUsingOneGatePerEdge) break;
                        }

                        break;
                    }

                case Direction.Right: {
                        (Celltype[] rightcells, Vector2 rightstart, Vector2 rightend) = GetRightEdge(curChunk);
                        (Celltype[] leftcells, Vector2 leftstart, Vector2 leftend) = GetLeftEdge(chunk);

                        var minLen = Math.Min(rightcells.Length, leftcells.Length);
                        for (int i = 0; i < minLen; i++) {
                            if (rightcells[i] == Celltype.Wall || leftcells[i] == Celltype.Wall)
                                continue;

                            Node curChunkGate = new(new Vector2(rightstart.Row + i, rightstart.Col), Connections: []);
                            Node chunkGate = new(new Vector2(leftstart.Row + i, leftstart.Col), Connections: []);

                            curChunkGate.Connections.Add(chunkGate);
                            chunkGate.Connections.Add(curChunkGate);

                            curChunk.Gates.Add(curChunkGate);
                            chunk.Gates.Add(chunkGate);
                            if (isUsingOneGatePerEdge) break;

                        }

                        break;
                    }

                case Direction.Up: {
                        (Celltype[] upcells, Vector2 upstart, Vector2 upend) = GetUpEdge(curChunk);
                        (Celltype[] downcells, Vector2 downstart, Vector2 downend) = GetDownEdge(chunk);

                        var minLen = Math.Min(upcells.Length, downcells.Length);
                        for (int i = 0; i < minLen; i++) {
                            if (upcells[i] == Celltype.Wall || downcells[i] == Celltype.Wall)
                                continue;

                            Node curChunkGate = new(new Vector2(upstart.Row, upstart.Col + i), Connections: []);
                            Node chunkGate = new(new Vector2(downstart.Row, downstart.Col + i), Connections: []);

                            curChunkGate.Connections.Add(chunkGate);
                            chunkGate.Connections.Add(curChunkGate);

                            curChunk.Gates.Add(curChunkGate);
                            chunk.Gates.Add(chunkGate);
                            if (isUsingOneGatePerEdge) break;

                        }

                        break;
                    }

                case Direction.Down: {
                        (Celltype[] downcells, Vector2 downstart, Vector2 downend) = GetDownEdge(curChunk);
                        (Celltype[] upcells, Vector2 upstart, Vector2 upend) = GetUpEdge(chunk);

                        var minLen = Math.Min(downcells.Length, upcells.Length);
                        for (int i = 0; i < minLen; i++) {
                            if (downcells[i] == Celltype.Wall || upcells[i] == Celltype.Wall)
                                continue;

                            Node curChunkGate = new(new Vector2(downstart.Row, downstart.Col + i), Connections: []);
                            Node chunkGate = new(new Vector2(upstart.Row, upstart.Col + i), Connections: []);

                            curChunkGate.Connections.Add(chunkGate);
                            chunkGate.Connections.Add(curChunkGate);

                            curChunk.Gates.Add(curChunkGate);
                            chunk.Gates.Add(chunkGate);
                            if (isUsingOneGatePerEdge) break;

                        }

                        break;
                    }
            }
        }

        private (Celltype[], Vector2, Vector2) GetLeftEdge(Chunk curChunk) {
            int rowStart = curChunk.Corner1.Row;
            int rowCount = curChunk.Corner2.Row - curChunk.Corner1.Row;
            int col = curChunk.Corner1.Col;

            var edgeCells = Enumerable.Range(rowStart, rowCount)
                .Select(r => cells[r, col])
                .ToArray();

            Vector2 start = new Vector2(rowStart, col);
            Vector2 end = new Vector2(curChunk.Corner2.Row - 1, col);

            return (edgeCells, start, end);
        }

        private (Celltype[], Vector2, Vector2) GetRightEdge(Chunk curChunk) {
            int rowStart = curChunk.Corner1.Row;
            int rowCount = curChunk.Corner2.Row - curChunk.Corner1.Row;
            int col = curChunk.Corner2.Col - 1;

            var edgeCells = Enumerable.Range(rowStart, rowCount)
                .Select(r => cells[r, col])
                .ToArray();

            Vector2 start = new Vector2(rowStart, col);
            Vector2 end = new Vector2(curChunk.Corner2.Row - 1, col);

            return (edgeCells, start, end);
        }

        private (Celltype[], Vector2, Vector2) GetUpEdge(Chunk curChunk) {
            int colStart = curChunk.Corner1.Col;
            int colCount = curChunk.Corner2.Col - curChunk.Corner1.Col;
            int row = curChunk.Corner1.Row;

            var edgeCells = Enumerable.Range(colStart, colCount)
                .Select(c => cells[row, c])
                .ToArray();

            Vector2 start = new Vector2(row, colStart);
            Vector2 end = new Vector2(row, curChunk.Corner2.Col - 1);

            return (edgeCells, start, end);
        }

        private (Celltype[], Vector2, Vector2) GetDownEdge(Chunk curChunk) {
            int colStart = curChunk.Corner1.Col;
            int colCount = curChunk.Corner2.Col - curChunk.Corner1.Col;
            int row = curChunk.Corner2.Row - 1;

            var edgeCells = Enumerable.Range(colStart, colCount)
                .Select(c => cells[row, c])
                .ToArray();

            Vector2 start = new Vector2(row, colStart);
            Vector2 end = new Vector2(row, curChunk.Corner2.Col - 1);

            return (edgeCells, start, end);
        }

        private Direction GetDirection((int, int) dir) {
            if (dir == (0, 1)) return Direction.Right;
            if (dir == (1, 0)) return Direction.Down;
            if (dir == (0, -1)) return Direction.Left;
            if (dir == (-1, 0)) return Direction.Up;
            throw new Exception("Invalid direction");
        }

        public Chunk GetChunk(Vector2 coord) => chunks[coord.Row / gridSize, coord.Col / gridSize];

        public IEnumerable<Node> GetAllGates() {
            var seen = new HashSet<Node>();
            for (int i = 0; i < chunks.GetLength(0); i++) {
                for (int j = 0; j < chunks.GetLength(1); j++) {
                    foreach (var gate in chunks[i, j].Gates)
                        if (seen.Add(gate))
                            yield return gate;
                }
            }
        }

        internal GridMap InitConnections(IAlgo algo) {
            for (int i = 0; i < chunks.GetLength(0); i++) {
                for (int j = 0; j < chunks.GetLength(1); j++) {
                    var chunk = chunks[i, j];
                    var gates = chunk.Gates;

                    if (gates.Count < 2)
                        continue;

                    // Build local graph for this chunk once
                    var nodes = ChunkToNodes(chunk);

                    for (int u = 0; u < gates.Count; u++) {
                        if (!nodes.TryGetValue(GetLocalVector2(chunk, gates[u].Pos), out var startNode))
                            continue;

                        for (int v = u + 1; v < gates.Count; v++) {
                            if (!nodes.TryGetValue(GetLocalVector2(chunk, gates[v].Pos), out var goalNode))
                                continue;

                            FinalPath res = algo.FindGoal(startNode, goalNode);

                            if (res.path.Count > 0) {
                                if (!gates[u].Connections.Contains(gates[v]))
                                    gates[u].Connections.Add(gates[v]);

                                if (!gates[v].Connections.Contains(gates[u]))
                                    gates[v].Connections.Add(gates[u]);
                            }
                        }
                    }
                }
            }

            return this;
        }


        private Vector2 GetLocalVector2(Chunk chunk, Vector2 pos) {
            Debug.Assert(chunk != null);
            Debug.Assert(pos.Row >= chunk.Corner1.Row && pos.Row < chunk.Corner2.Row, message: $"input vector2 out of bounds row : {pos.Row}");
            Debug.Assert(pos.Col >= chunk.Corner1.Col && pos.Col < chunk.Corner2.Col);

            return new Vector2(pos.Row - chunk.Corner1.Row, pos.Col - chunk.Corner1.Col);
        }

        private Celltype[,] GetLocalChunk(Chunk chunk) {

            int rowLen = chunk.Corner2.Row - chunk.Corner1.Row;
            int colLen = chunk.Corner2.Col - chunk.Corner1.Col;
            Celltype[,] output = new Celltype[rowLen, colLen];

            for (int i = 0; i < rowLen; i++) {
                for (int j = 0; j < colLen; j++) {
                    output[i, j] = cells[chunk.Corner1.Row + i, chunk.Corner1.Col + j];
                }
            }
            
            return output;
        }
        private Dictionary<Vector2, Node> ChunkToNodes(Chunk chunk) {
            Celltype[,] chunkMap = GetLocalChunk(chunk);
            int rows = chunkMap.GetLength(0);
            int cols = chunkMap.GetLength(1);

            var nodes = new Dictionary<Vector2, Node>();

            // create walkable cell nodes
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    if (chunkMap[r, c] == Celltype.Wall)
                        continue;

                    var pos = new Vector2(r, c);
                    nodes[pos] = new Node(pos, new List<Node>());
                }
            }

            // connect cell neighbors
            var dirs = new (int dr, int dc)[] {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            foreach (var node in nodes.Values) {
                foreach (var (dr, dc) in dirs) {
                    var nextPos = new Vector2(node.Pos.Row + dr, node.Pos.Col + dc);
                    if (nodes.TryGetValue(nextPos, out var neighbor)) {
                        if (!node.Connections.Contains(neighbor))
                            node.Connections.Add(neighbor);
                    }
                }
            }

            // attach gates
            foreach (var gate in chunk.Gates) {
                var localPos = GetLocalVector2(chunk, gate.Pos);

                if (!nodes.TryGetValue(localPos, out var cellNode))
                    throw new InvalidOperationException($"Gate at {gate.Pos} is not on a walkable cell.");

                // connect gate node to the cell node
                if (!gate.Connections.Contains(cellNode))
                    gate.Connections.Add(cellNode);

                if (!cellNode.Connections.Contains(gate))
                    cellNode.Connections.Add(gate);
            }

            return nodes;
        }


    // Problems :
    // - 
    public FinalPath GetGridPath(IAlgo algo) {
            if (start == null || goal ==null) {
                throw new Exception();
            }
            Chunk startChunk = GetChunk(start);
            Chunk goalChunk = GetChunk(goal);

            // find connected gate to the starting node
            (Node startingGate, FinalPath startingPath) = GetConnectedGate(startChunk, start, algo);
            // Similarly find the connected gate to the goal node
            (Node goalGate, FinalPath goalPath) = GetConnectedGate(goalChunk, goal, algo);

            FinalPath chunkPath = algo.FindGoal(startingGate, goalGate);
            //List<(Chunk c, Node entry, Node exit)> pairs = [];
            var a = new HashSet<Chunk>();
            foreach (Node node in chunkPath.nodes) {
                a.Add(GetChunk(node.Pos));
            }
            List<FinalPath> paths = [startingPath];

            var pathnodes = chunkPath.nodes;
            for (int i = 1; i < pathnodes.Count -2; i=i+2) {

                Node gate1 = pathnodes[i];
                Node gate2 = pathnodes[i+1];

                Chunk chunk = GetChunk(gate1.Pos);
                //Chunk chunk2 = GetChunk(gate2.Pos);
                var chunkmap = ChunkToNodes(chunk);

                var localPos1 = chunkmap[GetLocalVector2(chunk, gate1.Pos)];
                var localPos2 = chunkmap[GetLocalVector2(chunk, gate2.Pos)];

                FinalPath localPath = algo.FindGoal(localPos1, localPos2); // this is the localpath it needs to become a globalpath
                var globaPath = ToGlobalPath(chunk, localPath);
                paths.Add(globaPath);
            }
            paths.Add(goalPath);

            FinalPath result = new();
            foreach (FinalPath path in paths) {
                result.path.AddRange(path.path);
                result.animationSteps.AddRange(path.animationSteps);
                result.nodes.AddRange(path.nodes);
            }
            return result;

    }
        private FinalPath ToGlobalPath(Chunk chunk, FinalPath localPath) {
            var result = new FinalPath();

            foreach (var pos in localPath.path) {
                result.AddPath(new Vector2(
                    pos.Row + chunk.Corner1.Row,
                    pos.Col + chunk.Corner1.Col
                ));
            }

            foreach (var node in localPath.nodes) {
                result.AddNode(new Node(
                    new Vector2(
                        node.Pos.Row + chunk.Corner1.Row,
                        node.Pos.Col + chunk.Corner1.Col
                    ),
                    new List<Node>()
                ));
            }

            foreach (var step in localPath.animationSteps) {
                result.AddAnimationStep(
                    new Vector2(
                        step.pos.Row + chunk.Corner1.Row,
                        step.pos.Col + chunk.Corner1.Col
                    ),
                    step.isVisited,
                    step.isPath
                );
            }

            return result;
        }
        private (Node Gate, FinalPath Path) GetConnectedGate(Chunk chunk, Vector2 pos, IAlgo algo) {
            List<Node> gates = chunk.Gates;
            var map = ChunkToNodes(chunk);
            var localPos = GetLocalVector2(chunk, pos);
            var startingNode = map[localPos];
            int pathLength = int.MaxValue;
            FinalPath path = new();
            Node? closestGate = null;
            Console.WriteLine($"Chunk: {chunk.Corner1} -> {chunk.Corner2}");
            Console.WriteLine($"Local pos: {localPos}");
            Console.WriteLine($"Gates: {gates.Count}");
            foreach (Node gate in gates) {
                
                Console.WriteLine($"Testing gate {gate.Pos}");
                Console.WriteLine($"Local gate pos: {GetLocalVector2(chunk, gate.Pos)}");
                Console.WriteLine($"Path count: {path.path.Count}");
                path = algo.FindGoal(startingNode, gate);
                if (path.path.Count <= 0)
                    continue;
                if (path.path.Count < pathLength || closestGate == null) {
                    closestGate = gate;
                    pathLength = path.path.Count;
                
                }
            }
            if (closestGate == null || path == null) 
                throw new Exception();    
            
            return (closestGate, path);
        }

        private Node ChooseNearestGate(Chunk startChunk, Vector2 start, IAlgo algo) {
        List<Node> nodes = startChunk.Gates;

        var map = ChunkToNodes(startChunk);
        var localStart = GetLocalVector2(startChunk, start);

        Node startNodeOrphan = new(localStart, []);
        Node startNodeMap = map[localStart];

        foreach (Node node in nodes) {
            FinalPath path = algo.FindGoal(startNodeMap, map[GetLocalVector2(startChunk,node.Pos)]);
            if (path.path.Count > 0) {
                startNodeOrphan.Connections.Add(node);
                node.Connections.Add(startNodeOrphan);
            }
        }
        return startNodeOrphan;
    }
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