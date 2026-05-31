using System;
using System.Collections.Generic;
namespace HPF.model;

public class Manager {
    IAlgo selectedAlgo;
    PMap map;

    public Manager(IAlgo algo, PMap map) {
        this.selectedAlgo = algo;
        this.map = map;
    }

    public FinalPath Run() {
        var nodes = map.ToNodes(map.cells);   // build the full graph once
        var startNode = nodes[map.start];
        var goalNode = nodes[map.goal];


        return selectedAlgo.FindGoal(startNode, goalNode);
    }

    //    => selectedAlgo.FindGoal(map.cells, map.start, map.goal);
}