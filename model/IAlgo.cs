using System.Collections.Generic;

namespace HPF.model {
    public interface IAlgo {
        FinalPath FindGoal(PMap map);
    }

    public class BFS : IAlgo {
        public FinalPath FindGoal(PMap map) {
            var result = new FinalPath();

            var queue = new Queue<Coord>();
            var visited = new HashSet<Coord>();
            var parent = new Dictionary<Coord, Coord?>(); // child -> parent (null for start)

            queue.Enqueue(map.Start());
            visited.Add(map.Start());
            parent[map.Start()] = null;

            while (queue.Count > 0) {
                var current = queue.Dequeue();

                // mark visited animation step
                result.AddAnimationStep(current, isVisited: true, isPath: false);

                if (current == map.Goal) {
                    // reconstruct path
                    var path = new List<Coord>();
                    Coord? cur = current;

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

        private IEnumerable<Coord> GetNeighbors(PMap map, Coord c) {
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

                // You need a way to check whether a cell is a wall.
                // Since your PMap currently doesn't expose cells,
                // we’ll assume you’ll add an IsWall(row,col) helper (see below).
                if (!map.IsWall(nr, nc))
                    yield return new Coord(nr, nc);
            }
        }

        private bool InBounds(PMap map, int r, int c) {
            // assumes PMap has N and M exposed (see below).
            return r >= 0 && c >= 0 && r < map.N && c < map.M;
        }
    }
}
