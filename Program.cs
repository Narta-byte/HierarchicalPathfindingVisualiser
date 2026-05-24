// public static class Program
// {
//     public static void Main()
//     {
//         BFS bfs = new BFS();
//         AStar aStar = new AStar();

//         Stopwatch stopwatch = new Stopwatch();

    
//         int n = 32, m = 32;

//         var generatedMap = 
//             new MapGenBuilder()
//                 .SetMapSize(n,m)
//                 .WithisStartRestrictedToEdge(false)
//                 .BuildLabyrinth();
//         Console.WriteLine(generatedMap.MapString);

//         stopwatch.Start();

        
//         GridMap gridmapv2 = 
//             new GridMapBuilder()
//                 .WithMapSize(generatedMap.Rows,generatedMap.Cols)
//                 .WithGridSize(4)
//                 .WithMap(generatedMap.MapString)
//                 .WithOneGatePerEdge(false)
//                 .Build();
      

//         stopwatch.Stop();
//         Console.WriteLine($"Precomputing Time : {stopwatch.ElapsedMilliseconds} ms");

//         //ChunkVisualizer.PrintChunksWithGates(gridmapv2);
//         //ConnectionVisualizer.PrintConnections(gridmapv2);

//         //PrintPathAsAscii(gridmap, pa.path);
//         //var path = gridmapv2.GetGridPath(aStar);
//         //Visualizers.AnimateAsAscii(gridmapv2, path, delayMs: 80);

//         //ChunkVisualizer.PrintChunksWithGates(gridmapv2);

//         FinalPath p = new();
//         for (int i = 0; i < 3; i++) {
//             p = gridmapv2.GetGridPath(bfs);

//         }
//         stopwatch.Stop();
//         Console.WriteLine($"Time taken gridv2: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
//         stopwatch.Restart();
//         p = new();
//         for (int i = 0; i < 3; i++) {
//             p = gridmapv2.GetGridPath(aStar);
//             //FinalPath result = manager.Run();

//         }
//         stopwatch.Stop();
//         Console.WriteLine($"Time taken gridv2 with Astar: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");

//         //Visualizers.AnimateAsAscii(gridmap, path, delayMs: 80);
//         //ConnectionVisualizer.PrintConnections(gridmap);
//         var pmap = new PMap(n, m);
//         pmap.MapFromStr(generatedMap.Rows, generatedMap.Cols, generatedMap.MapString);
//         var manager = new Manager(bfs, pmap);

//         var nodes = pmap.ToNodes(pmap.cells);   // build the full graph once
//         var startNode = nodes[pmap.start ?? new Vector2(0,0)];
//         var goalNode = nodes[pmap.goal ?? new Vector2(0, 0)];
//         stopwatch.Restart();

//         //Visualizers.AnimateAsAscii(pmap,algo.FindGoal(startNode, goalNode), delayMs: 80);

//         for (int i = 0; i < 3; i++) { 
//             p = bfs.FindGoal(startNode, goalNode);
//         }

//         stopwatch.Stop();
//         Console.WriteLine($"Time taken simple: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
//         stopwatch.Restart();
//         for (int i = 0; i < 3; i++) {
//             p = aStar.FindGoal(startNode, goalNode);
//         }

//         stopwatch.Stop();
//         Console.WriteLine($"Time taken simple Astar: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
//     }

    
// }