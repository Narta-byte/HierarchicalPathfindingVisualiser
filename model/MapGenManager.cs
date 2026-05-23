namespace HPF.model;
public class MapGenManager {
    private int maxRow;
    private int maxCol;
    public MapGenManager SetMapSize(int newMaxRow, int maxHeight) {
        if (newMaxRow % 2 == 0) newMaxRow++;
        if (maxHeight % 2 == 0) maxHeight++;
        maxRow = newMaxRow;
        maxCol = maxHeight;
        return this;
    }
        
    public string GenerateLabyrinth(bool isStartRestrictedToEdge = false) {
        LabyrinthGenerator labyrinthGenerator = new LabyrinthGenerator();
        return labyrinthGenerator.Generate(maxRow, maxCol, isStartRestrictedToEdge)
                                 .ToFlatString();
    }
    public string GenerateNoiseMap() {
        return "";
    }
}

    

