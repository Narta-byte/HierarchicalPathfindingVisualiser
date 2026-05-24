using HPF.model;
using HPF.Web.Models;

namespace HPF.Web.Services;

public class AppState
{
    public AppConfig Config { get; set; } = new();

    public GeneratedMap? CurrentMap { get; private set; }

    public event Action? OnChange;

    public void GenerateMap()
    {
        var cfg = Config;

        var builder = new MapGenBuilder()
            .SetMapSize(cfg.Rows, cfg.Cols)
            .WithisStartRestrictedToEdge(cfg.RestrictStartToEdge);

        CurrentMap = cfg.Generator switch
        {
            "Labyrinth" =>
                builder.BuildLabyrinth(),

            "Noise" =>
                builder.GenerateNoiseMap(cfg.NoiseLevel),

            "Hardcoded" =>
                builder.BuildHardcodedMap(cfg.HardcodedMapId),

            _ =>
                builder.BuildLabyrinth()
        };

        Notify();
    }

    private void Notify()
    {
        OnChange?.Invoke();
    }
}