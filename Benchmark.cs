//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Running;
//using HPF.model;

//[SimpleJob(warmupCount: 3, iterationCount: 5)]
//[MemoryDiagnoser]
//public class PathfinderBenchmarks {
//    private PMap pmap;
//    private GridMap gridmap;
//    private IAlgo algo;

//    [GlobalSetup]
//    public void Setup() {
//        int n = 8, m = 16;
//        string mapStr =
//            ".S.#........#..." +
//            "...#...#....#..." +
//            "##.###.#....#..." +
//            ".....#.#....####" +
//            "...#.#.#....#..." +
//            "...#.#.#....#..G" +
//            "...#.#.#....#..." +
//            ".......#........";

//        algo = new BFS(); // or BFS/DFS
//        pmap = new PMap(n, m);
//        pmap.MapFromStr(n, m, mapStr);

//        gridmap = new GridMap(n, m, 4);
//        gridmap.MapFromStr(n, m, mapStr);
//        gridmap.SetIsUsingOneGatePerEdge(false)
//               .InitChunks()
//               .InitGates()
//               .InitConnections(algo);
//    }

//    [Benchmark]
//    public FinalPath PMapPathfinding() {
//        return pmap.Run(algo);
//    }

//    [Benchmark]
//    public FinalPath GridMapPathfinding() {
//        return gridmap.GetGridPath(algo);
//    }
//}

//public class Program {
//    public static void Main(string[] args) {
//        BenchmarkRunner.Run<PathfinderBenchmarks>();
//    }
//}
//using System;

//public class Class1
//{
//	public Class1()
//	{
//	}
//}
