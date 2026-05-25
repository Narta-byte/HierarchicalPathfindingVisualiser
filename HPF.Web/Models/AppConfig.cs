namespace HPF.Web;

public class AppConfig
{
    public int Rows { get; set; } = 64;
    public int Cols { get; set; } = 64;

    public int ChunkSize { get; set; } = 4;

    public bool RestrictStartToEdge { get; set; }
    public bool OneGatePerEdge { get; set; }

    public bool ShowChunks { get; set; } = false;
    public bool ShowGates { get; set; } = false;
    public bool ShowConnections { get; set; }

    public string Algorithm { get; set; } = "AStar";

    // ---- MISSING FIELDS (required by AppState) ----

    public string Generator { get; set; } = "Labyrinth";

    public string NoiseMap { get; set; } = "Noise";

    public string GateAlgorithm { get; set; } = "AStar";

    // separate algorithms
    public string PathAlgorithm { get; set; } = "AStar";

    public double NoiseLevel { get; set; } = 0.30;

    public int HardcodedMapId { get; set; } = 0;
}