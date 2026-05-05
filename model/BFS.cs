using HPF.model;
using static HPF.model.PMap;

namespace HPF.model {
    public class BFS : IAlgo {
        public FinalPath FindGoal(Celltype[,] map, Vector2 start, Vector2 goal) {
            var result = new FinalPath();

            var queue = new Queue<Vector2>();
            var visited = new HashSet<Vector2>();
            var parent = new Dictionary<Vector2, Vector2?>(); // child -> parent (null for start)

            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;

            while (queue.Count > 0) {
                var current = queue.Dequeue();

                // mark visited animation step
                result.AddAnimationStep(current, isVisited: true, isPath: false);

                if (current == goal) {
                    // reconstruct path
                    var path = new List<Vector2>();
                    Vector2? cur = current;

                    while (cur != null) {
                        path.Add(cur);
                        cur = parent[cur];
                    }

                    path.Reverse();

                    foreach (var p in path) {
                        result.AddPath(p);
                        result.AddAnimationStep(p, isVisited: false, isPath: true);
                    }

                    return result;
                }

                foreach (var next in GetNeighbors(map, current)) {
                    if (visited.Contains(next))
                        continue;

                    visited.Add(next);
                    parent[next] = current;
                    queue.Enqueue(next);
                }
            }

            // no path found: return empty FinalPath
            return result;
        }

        private IEnumerable<Vector2> GetNeighbors(Celltype[,] map, Vector2 c) {
            // 4-directional movement
            var dirs = new (int dr, int dc)[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            foreach (var (dr, dc) in dirs) {
                var nr = c.Row + dr;
                var nc = c.Col + dc;

                if (!InBounds(map, nr, nc))
                    continue;


                if (map[nr, nc] != Celltype.Wall)
                    yield return new Vector2(nr, nc);
            }
        }

        private bool InBounds(Celltype[,] map, int r, int c) {
            // assumes PMap has N and M exposed (see below).
            return r >= 0 && c >= 0 && r < map.GetLength(0) && c < map.GetLength(1);
        }
    }
}