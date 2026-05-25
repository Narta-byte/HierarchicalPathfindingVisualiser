using HPF.model;
namespace HPF.Web.Models;
public class GridMapDto {
        public int GridSize { get; set; }
        public List<GateDto> AllGates { get; set; } = new();
    }

public class GateDto {
    public Vector2 Pos { get; set; }
    public List<Vector2> Connections { get; set; } = new();
}
public class FinalPathDto
{
    public List<AnimationStepDto> AnimationSteps { get; set; } = new();
}

public class AnimationStepDto
{
    public Vector2 Pos { get; set; }

    public bool IsVisited { get; set; }

    public bool IsPath { get; set; }
}

