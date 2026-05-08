using System.Collections.Generic;

namespace HPF.model {
    public class BFS : IAlgo {
        public FinalPath FindGoal(Node start, Node goal) {
            var result = new FinalPath();

            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node?>();

            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;
            //Console.WriteLine("######### BFS #########");
            while (queue.Count > 0) {
                var current = queue.Dequeue();
                //Console.WriteLine(current);
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

                    visited.Add(next);
                    parent[next] = current;
                    queue.Enqueue(next);
                }
            }

            return result;
        }
    }
}
