namespace HPF.model
{
    public class PMap
    {
        public enum Celltype
        {
            Empty,
            Wall,
            Start,
            Goal
        }

        int n;
        int m;
        public Coord start;
        public Coord goal;
        public Celltype[,] cells = new Celltype[0, 0];

        public int N => n;
        public int M => m;

        public Coord Start() => start;
        public Coord Goal => goal;

        public bool IsWall(int row, int col)
            => cells[row, col] == Celltype.Wall;

        public PMap(int n, int m)
        {
            this.n = n;
            this.m = m;
            cells = new Celltype[n,m];
        }
        public void MapFromStr(int n, int m, string mapStr)
        {
                this.n = n;
                this.m = m;
                cells = new Celltype[n,m];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        char c = mapStr[i * m + j];
                        switch (c)
                        {
                            case 'S':
                                cells[i, j] = Celltype.Start;
                                start = new Coord(Row:i,Col: j);
                                break;
                            case 'G':
                                cells[i, j] = Celltype.Goal;
                                goal = new Coord(Row: i, Col: j);
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