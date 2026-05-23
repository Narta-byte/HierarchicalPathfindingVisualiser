namespace HPF.model;
public class MapGenBuilder {
    private int maxRow;
    private int maxCol;
    public MapGenBuilder SetMapSize(int newMaxRow, int maxHeight) {
        if (newMaxRow % 2 == 0) newMaxRow++;
        if (maxHeight % 2 == 0) maxHeight++;
        maxRow = newMaxRow;
        maxCol = maxHeight;
        return this;
    }
        
    public string BuildLabyrinth(bool isStartRestrictedToEdge = false) {
        if (maxRow==0 || maxCol == 0)
            throw new Exception($"Map size not set");
        LabyrinthGenerator labyrinthGenerator = new LabyrinthGenerator();
        return labyrinthGenerator.Generate(maxRow, maxCol, isStartRestrictedToEdge)
                                 .ToFlatString();
    }
    public string GenerateNoiseMap() {
        return "";
    }
}

    

