using System.Collections.Generic;
using static HPF.model.PMap;

namespace HPF.model {
    public interface IAlgo {
        FinalPath FindGoal(Celltype[,] map, Vector2 start, Vector2 goal);
    }
}

    
