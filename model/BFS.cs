using System.Collections.Generic;

namespace HPF.model {
    public class BFS : IAlgo {
        public FinalPath FindGoal(Node start, Node goal, Func<Node, bool> isValidNode = null) {
            var result = new FinalPath();

            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node?>();

            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;
            while (queue.Count > 0) {
                var current = queue.Dequeue();
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
                    if (isValidNode == null || isValidNode(next))
                        {
                        //var a = 1;
                        //if (isValidNode != null){
                        //    var b = isValidNode(next);
                        //}
                        visited.Add(next);
                        parent[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }

            return result;
        }
    }
}
