using System;
using System.Collections.Generic;
namespace HPF.model;

public class Manager
{
    IAlgo selectedAlgo;
    PMap map;
    GridMap gridmap;

    public Manager(IAlgo algo, PMap map, GridMap gridmap)
    {
        this.selectedAlgo = algo;
        this.map = map;
        this.gridmap = gridmap;
    }

    public FinalPath Run()
        => selectedAlgo.FindGoal(map.cells, map.start, map.goal);
}

