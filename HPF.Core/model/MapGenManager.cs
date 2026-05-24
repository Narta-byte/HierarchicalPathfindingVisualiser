using System.Data;

namespace HPF.model;
public class GeneratedMap
{
    public int Rows { get; init; }
    public int Cols { get; init; }
    public string MapString { get; init; } = "";
}
public class MapGenBuilder {
    private int maxRow;
    private int maxCol;
    private bool _isStartRestrictedToEdge = true;
    public MapGenBuilder SetMapSize(int newMaxRow, int maxHeight) {
        if (newMaxRow % 2 == 0) newMaxRow++;
        if (maxHeight % 2 == 0) maxHeight++;
        maxRow = (newMaxRow + 1 ) /2;
        maxCol = (maxHeight + 1) /2;
        return this;
    }
    public MapGenBuilder WithisStartRestrictedToEdge(bool isStartRestrictedToEdge = true) {
        _isStartRestrictedToEdge = isStartRestrictedToEdge;
        return this;
    }
    public GeneratedMap BuildLabyrinth() {
        if (maxRow == 0 || maxCol == 0)
            throw new Exception("Map size not set");

        string map = new LabyrinthGenerator()
            .Generate(maxRow, maxCol, _isStartRestrictedToEdge)
            .ToFlatString();

        return new GeneratedMap
        {
            Rows = maxRow * 2 - 1,
            Cols = maxCol * 2 - 1,
            MapString = map
        };
    }
    public GeneratedMap GenerateNoiseMap(double noiseLevel = 0.30) {
        string mapStr = NoisyMapGenerator.Generate(maxRow, maxCol, wallChance: noiseLevel);

        return new GeneratedMap {
            Rows = maxRow,
            Cols = maxCol,
            MapString = mapStr
        };
    }

    public GeneratedMap BuildHardcodedMap(int cnt = 0) {
        return cnt switch
        {
            0 => new GeneratedMap
            {
                Rows = 16,
                Cols = 32,
                MapString =
                    ".S.#...#....#......#........#..." +
                    "...#.#.#....#......#........#..." +
                    "...#.#.#....#......#........#..." +
                    ".....#.#....#......#........#..." +
                    "...###.#....#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#........#......#........#..." +
                    "...#...........................G"
            },

            1 => new GeneratedMap
            {
                Rows = 8,
                Cols = 16,
                MapString =
                    "S..#........#..." +
                    "...#........#..." +
                    "...#........#..." +
                    "...#........####" +
                    "...#............" +
                    "...#...........G" +
                    "................" +
                    "...#............"
            },

            2 => new GeneratedMap
            {
                Rows = 8,
                Cols = 16,
                MapString =
                    "............#..." +
                    "...#...#....#..." +
                    "##S#...#....#..." +
                    "...#...#....####" +
                    "...#...#....#..." +
                    "...#...#....#..G" +
                    "...#...#....#..." +
                    ".......#........"
            },

            3 => new GeneratedMap
            {
                Rows = 8,
                Cols = 16,
                MapString =
                    ".S.#........#..." +
                    "...#...#....#..." +
                    "##.###.#....#..." +
                    ".....#.#....####" +
                    "...#.#.#....#..." +
                    "...#.#.#....#..G" +
                    "...#.#.#....#..." +
                    ".......#........"
            },

            4 => new GeneratedMap
            {
                Rows = 8,
                Cols = 16,
                MapString =
                    ".S.#........#..." +
                    "...#...#....#..." +
                    "##.###.#....#..." +
                    ".....#.#....####" +
                    "...#.#.#....#..." +
                    "...#.#.#....#..G" +
                    "...#.#.#....#..." +
                    ".......#........"
            },

            _ => throw new ArgumentOutOfRangeException(
                nameof(cnt),
                cnt,
                "Unknown hardcoded map id")
        };
    }
}

    

