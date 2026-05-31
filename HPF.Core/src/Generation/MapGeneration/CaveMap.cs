using System.Data;
using System.Text;

namespace HPF.model;
// based on https://excaliburjs.com/blog/Cellular%20Automata/
// public record Rules(int Cutoff0, int Cutoff1, int ZeroLimit, int OneLimit);

public class CaveMap
{
    private enum OutOfBoundsRule {
        WALL,
        FLOOR,
        RANDOM,
        MIRROR
    }
    private const int Cutoff0 = 3;
    private const int Cutoff1 = 5;
    private const int ZeroLimit = 4;
    private const int OneLimit = 5;

    private OutOfBoundsRule oob = OutOfBoundsRule.WALL;
    public int rows;
    public int cols;
    private Random _random = new();
    private int[,] map;
    private int? seed = null;

    // private Rules rules;
    public CaveMap(int _rows, int _cols) {
        rows = _rows;
        cols = _cols;

        map = new int[rows, cols];

        // Fill with empty cells or walls
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                map[r, c] = _random.Next() % 2 == 0 ? 1 : 0;
            }
        }
        // rules = new Rules(
        //     Cutoff0: 3,
        //     Cutoff1: 3,
        //     ZeroLimit: 4,
        //     OneLimit: 5
        // );
    }
    public CaveMap SetSeed(int _seed) {
        seed = _seed;
        _random = new Random(Seed:_seed);
        return this;
    }
    public CaveMap SetRules(int? cutoff0 = null, int? cutoff1 = null, int zeroLimit = 4, int oneLimit = 5) {
        if (cutoff0 != null) 
            zeroLimit = (int) cutoff0 + 1;
        if (cutoff1!= null) 
            oneLimit = (int) cutoff1;
        // rules = new Rules(
        //     Cutoff0: cutoff0 ?? 1,
        //     Cutoff1: cutoff1 ?? 1,
        //     ZeroLimit: zeroLimit,
        //     OneLimit: oneLimit
        // );
        return this;
    }
    public CaveMap AppllyCellularAutomaton() {
        int[,] newMap = new int[rows, cols];
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                int wallCnt = CountAdjencentWalls(r,c);
                if (map[r,c] == 1) {
                    if (wallCnt < ZeroLimit) {
                        newMap[r,c] = 0;
                    } else {
                        newMap[r,c] = 1;
                    }
                } else {
                    if (wallCnt >= OneLimit) {
                        newMap[r,c] = 1;
                    } else {
                        newMap[r,c] = 0;
                    }
                }
            }
        }
        map = newMap;
        return this;
    }

    private int CountAdjencentWalls(int row, int col) {
        int cnt = 0;

        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0  && j == 0)
                    continue;

                int newRow = row + i;
                int newCol = col + j;

                if (0 <= newRow && newRow < rows && 0 <= newCol && newCol <  cols) {
                    cnt = map[newRow,newCol] == 1 ? (cnt+1) : cnt;
                } else {
                    switch (oob)
                    {
                        case OutOfBoundsRule.FLOOR:
                            break;
                        case OutOfBoundsRule.WALL:
                        cnt++;
                            break;
                        case OutOfBoundsRule.RANDOM:
                            cnt = _random.Next() % 2 == 0 ? (cnt+1) : cnt;
                            break;
                        case OutOfBoundsRule.MIRROR:
                            cnt = map[row,col] == 1 ? 1 : 0;
                            break;
                        default:
                            throw new Exception($"Invalid OutOfBoundsRule {oob}");
                    }
                }
            }
            
        }
        return cnt;
    }
    public CaveMap SetStartAndGoal() {
        // Build connected component labels via BFS
        int[,] labels = new int[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                labels[r, c] = -1;

        // component index -> list of (row, col) floor cells
        var components = new Dictionary<int, List<(int r, int c)>>();
        int nextLabel = 0;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = {  0, 0,-1, 1 };

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                if (map[r, c] != 0 || labels[r, c] != -1)
                    continue;

                // BFS from (r, c)
                int label = nextLabel++;
                var cells = new List<(int, int)>();
                var queue = new Queue<(int, int)>();
                queue.Enqueue((r, c));
                labels[r, c] = label;

                while (queue.Count > 0) {
                    var (cr, cc) = queue.Dequeue();
                    cells.Add((cr, cc));

                    for (int d = 0; d < 4; d++) {
                        int nr = cr + dr[d];
                        int nc = cc + dc[d];
                        if (nr < 0 || nr >= rows || nc < 0 || nc >= cols)
                            continue;
                        if (map[nr, nc] != 0 || labels[nr, nc] != -1)
                            continue;
                        labels[nr, nc] = label;
                        queue.Enqueue((nr, nc));
                    }
                }

                components[label] = cells;
            }
        }

        if (components.Count == 0)
            throw new InvalidOperationException("No floor cells found to place start and goal.");

        // Pick the largest component
        var largest = components
            .OrderByDescending(kv => kv.Value.Count)
            .First()
            .Value;

        if (largest.Count < 2)
            throw new InvalidOperationException("Largest component has fewer than 2 cells.");

        // Pick 2 distinct random positions
        int startIdx = _random.Next(largest.Count);
        int goalIdx;
        do { goalIdx = _random.Next(largest.Count); }
        while (goalIdx == startIdx);

        var (sr, sc) = largest[startIdx];
        var (gr, gc) = largest[goalIdx];

        map[sr, sc] = 2;
        map[gr, gc] = 3;

        return this;
    }
    public string ToMapStr() {
        StringBuilder sb = new();
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                sb.Append(map[r, c] switch {
                    1 => '#',
                    2 => 'S',
                    3 => 'G',
                    _ => '.'
                });
            }
        }
        return sb.ToString();
    }
}