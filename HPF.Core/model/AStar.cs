using System.Collections.Generic;

namespace HPF.model {
    public class AStar : IAlgo {
        public FinalPath FindGoal(Node start, Node goal, Func<Node, bool>? isValidNode = null) {
            var result = new FinalPath();

            var openSet = new PriorityQueue<Node, float>();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node?>();
            var gScore = new Dictionary<Node, float>();

            gScore[start] = 0;
            parent[start] = null;
            openSet.Enqueue(start, Heuristic(start, goal));

            while (openSet.Count > 0) {
                var current = openSet.Dequeue();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                result.AddAnimationStep(current.Pos, isVisited: true, isPath: false);

                if (current == goal) {
                    var path = new List<Node>();
                    Node? cur = current;

                    while (cur != null) {
                        path.Add(cur);
                        cur = parent[cur];
                    }

                    path.Reverse();

                    foreach (var p in path) {
                        result.AddPath(p.Pos);
                        result.AddNode(p);
                        result.AddAnimationStep(p.Pos, isVisited: false, isPath: true);
                    }

                    return result;
                }

                foreach (var next in current.Connections) {
                    if (visited.Contains(next))
                        continue;
                    if (isValidNode != null && !isValidNode(next))
                        continue;

                    float tentativeG = gScore[current] + Cost(current, next);

                    if (!gScore.ContainsKey(next) || tentativeG < gScore[next]) {
                        gScore[next] = tentativeG;
                        parent[next] = current;
                        float f = tentativeG + Heuristic(next, goal);
                        openSet.Enqueue(next, f);
                    }
                }
            }

            return result;
        }

        private float Heuristic(Node a, Node b) {
            // Manhattan distance
            return Math.Abs(a.Pos.Row - b.Pos.Row) + Math.Abs(a.Pos.Col - b.Pos.Col);
        }

        private float Cost(Node a, Node b) {
            // Uniform cost, adjust if your nodes have weights
            return 1f;
        }
    }
}
