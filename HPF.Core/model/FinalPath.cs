using System.Collections.Generic;

namespace HPF.model
{
    public class FinalPath
    {
        public List<Vector2> path { get; } = new();
        public List<Node> nodes { get; } = new();
        public List<AnimationStep> animationSteps { get; } = new();

        public void AddPath(Vector2 c) => path.Add(c);
        public void AddNode(Node node) => nodes.Add(node);

        public void AddAnimationStep(Vector2 pos, bool isVisited, bool isPath)
            => animationSteps.Add(new AnimationStep(pos, isVisited, isPath));
    }
}
