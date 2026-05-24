using HPF.model;

namespace HPF.Web;

public class AppConfig
{
    public int Rows { get; set; }
    public int Cols { get; set; }

    public int ChunkSize { get; set; }

    public bool RestrictStartToEdge { get; set; }
    public bool OneGatePerEdge { get; set; }

    public bool ShowChunks { get; set; }
    public bool ShowGates { get; set; }
    public bool ShowConnections { get; set; }
    public bool ShowPath { get; set; }

    public string Algorithm { get; set; } = "AStar";
}