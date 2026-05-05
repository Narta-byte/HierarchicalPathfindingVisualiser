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

        public void MapFromStr(int n, int m, string mapStr) {
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
        }


    }
}