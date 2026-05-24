using System.Collections.Generic;
using static HPF.model.PMap;

namespace HPF.model {
    public interface IAlgo {
        //FinalPath FindGoal(Node start, Node goal);
        FinalPath FindGoal(Node start, Node goal, Func<Node, bool>? isValidNode = null);

    }
}

    
