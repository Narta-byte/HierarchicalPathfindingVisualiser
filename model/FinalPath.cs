using System.Collections.Generic;

namespace HPF.model
{
    public class FinalPath
    {
        public List<Coord> path { get; } = new();
        public List<AnimationStep> animationSteps { get; } = new();

        public void AddPath(Coord c) => path.Add(c);

        public void AddAnimationStep(Coord pos, bool isVisited, bool isPath)
            => animationSteps.Add(new AnimationStep(pos, isVisited, isPath));
    }
}
