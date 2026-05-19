using static HPF.model.PMap;

namespace HPF.model {
    public class DFS : IAlgo {
        //public FinalPath FindGoal(Celltype[,] map, Vector2 start, Vector2 goal) {
        //    throw new NotImplementedException();
        //}
        public FinalPath FindGoal(Node start, Node goal, Func<Node, bool>? isValidNode = null) {
            var result = new FinalPath();

            var stack = new Stack<Node>();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node?>();

            stack.Push(start);
            visited.Add(start);
            parent[start] = null;
            while (stack.Count > 0) {
                var current = stack.Pop();
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
                        continue;
                    visited.Add(next);
                    parent[next] = current;
                    stack.Push(next);
                }
            }

            return result;
        }
    }
}

