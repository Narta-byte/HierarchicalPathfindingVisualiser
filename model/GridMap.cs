
namespace HPF.model
{
    public record Chunk(Vector2 Corner1, Vector2 Corner2, List<Gate> Gates);
    public enum Direction { Up, Down, Left, Right }
    public record Gate(Vector2 Coord, List<Gate> Connections);
 
    public class GridMap : PMap
    {
        public bool isUsingOneGatePerEdge = false;
        public void SetIsUsingOneGatePerEdge(bool toggle) => isUsingOneGatePerEdge = toggle;
        int gridSize = 4;
   
        public int GridSize => gridSize;

        Chunk[,] chunks = new Chunk[0, 0];

        public GridMap(int n, int m, int gridSize) : base(n, m) {
            this.gridSize = gridSize;

            int chunkRows = (N + this.gridSize - 1) / this.gridSize;
            int chunkCols = (M + this.gridSize - 1) / this.gridSize;

            chunks = new Chunk[chunkRows, chunkCols];
        }

        public void InitChunks() {
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
                        new List<Gate>()
                    );
                }
            }
        }

        public void InitGates() {
            List<(int,int)> directions = new List<(int,int)> { (0,1), (1,0)};
            for (int i = 0; i < chunks.GetLength(0); i++) {
                for (int j = 0; j < chunks.GetLength(1); j++) {
                    Chunk curChunk = chunks[i,j];
                    foreach ((int,int) dir in directions) {
                        int rowdir = i + dir.Item1;
                        int coldir = j + dir.Item2;
                        if (rowdir < 0 || rowdir >= chunks.GetLength(0) || coldir < 0 || coldir >= chunks.GetLength(1))
                            continue;


                            FindGates(curChunk, chunks[rowdir, coldir], dir);
                    }
                }
            }

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

                            Gate curChunkGate = new(new Vector2(leftstart.Row + i, leftstart.Col), Connections: []);
                            Gate chunkGate = new(new Vector2(rightstart.Row + i, rightstart.Col), Connections: []);

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

                            Gate curChunkGate = new(new Vector2(rightstart.Row + i, rightstart.Col), Connections: []);
                            Gate chunkGate = new(new Vector2(leftstart.Row + i, leftstart.Col), Connections: []);

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

                            Gate curChunkGate = new(new Vector2(upstart.Row, upstart.Col + i), Connections: []);
                            Gate chunkGate = new(new Vector2(downstart.Row, downstart.Col + i), Connections: []);

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

                            Gate curChunkGate = new(new Vector2(downstart.Row, downstart.Col + i), Connections: []);
                            Gate chunkGate = new(new Vector2(upstart.Row, upstart.Col + i), Connections: []);

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
            if (dir == (0,1)) return Direction.Right;
            if (dir == (1,0)) return Direction.Down;
            if (dir == (0,-1)) return Direction.Left;
            if (dir == (-1,0)) return Direction.Up;
            throw new Exception("Invalid direction");
        }

        public Chunk GetChunk(Vector2 coord) => chunks[coord.Row / gridSize, coord.Col / gridSize];

        public IEnumerable<Gate> GetAllGates() {
            var seen = new HashSet<Gate>();
            for (int i = 0; i < chunks.GetLength(0); i++) {
                for (int j = 0; j < chunks.GetLength(1); j++) {
                    foreach (var gate in chunks[i, j].Gates)
                        if (seen.Add(gate))
                            yield return gate;
                }
            }
        }

        internal void InitConnections(IAlgo algo) {
            // For each chunk:
            for (int i = 0;i < chunks.GetLength(0);i++) {
                for (int j = 0; i < chunks.GetLength(1); j++) {
                    var chunk = chunks[i, j];
                    //   - collect its gates
                    List<Gate> gates = chunk.Gates;
                    foreach (var gate in gates) { // foreach gate connect them VIKTOR
                        Celltype[,] chunkmap = GetChunkMap(chunk);
                        FinalPath res = algo.FindGoal(
                            GetChunkMap(chunk), 
                        )
                    }
                }
            //   - for each gate, run a local search inside the chunk
            //   - find which other gates are reachable
            //   - store connections between reachable gates
        }

    }

        private Celltype[,] GetChunkMap(Chunk chunk) {

            int rowLen = chunk.Corner2.Row - chunk.Corner1.Row;
            int colLen = chunk.Corner2.Col - chunk.Corner1.Col;
            Celltype[,] output = new Celltype[rowLen, colLen];

            for (int i = 0; i < rowLen; i++) {
                for (int j = 0; j < colLen; j++) {
                    output[i,j] = cells[chunk.Corner1.Row+i,chunk.Corner1.Col + j];
                }
            }
            return output;
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