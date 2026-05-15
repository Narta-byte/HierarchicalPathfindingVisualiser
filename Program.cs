//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorPages();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();

//app.UseRouting();

//app.UseAuthorization();

//app.MapStaticAssets();
//app.MapRazorPages()
//   .WithStaticAssets();

//app.Run();
// ############################ Above is the default ASP.NET Core template code. Below is the actual code for your application. ############################


using BenchmarkDotNet.Disassemblers;
using HPF.model;
using System.Diagnostics;
using static HPF.model.Gate;

public static class Program
{
    public static void Main()
    {
        // 1) 
        IAlgo algo = new BFS();

        // 2) Create a map (S=start, G=goal, #=wall, .=empty)
        //    NOTE: MapFromStr expects ALL rows concatenated into one string.
        //int n = 7, m = 10;
        //string mapStr =
        //    "S....#...." +
        //    ".##..#...." +
        //    ".#...#..#." +
        //    ".#.#.....#" +
        //    ".#.#.###.#" +
        //    ".#...#...G" +
        //    ".....#....";

        int n = 8, m = 16;
        string mapStr =
            "S..#........#..." +
            "...#........#..." +
            "...#........#..." +
            "...#........####" +
            "...#............" +
            "...#...........G" +
            "................" +
            "...#............";
        //string mapStr =
        //   "............#..." +
        //   "...#...#....#..." +
        //   "##S#...#....#..." +
        //   "...#...#....####" +
        //   "...#...#....#..." +
        //   "...#...#....#..G" +
        //   "...#...#....#..." +
        //   ".......#........";
        //string mapStr =
        //   ".S.#........#..." +
        //   "...#...#....#..." +
        //   "##.###.#....#..." +
        //   ".....#.#....####" +
        //   "...#.#.#....#..." +
        //   "...#.#.#....#..G" +
        //   "...#.#.#....#..." +
        //   ".......#........";
        Stopwatch stopwatch = new Stopwatch();

        //int n = 16, m = 32;

        ////string mapStr =
        ////   ".S.#...#....#......#........#..." +
        ////   "...#.#.#....#......#........#..." +
        ////   "...#.#.#....#......#........#..." +
        ////   ".....#.#....#......#........#..." +
        ////   "...###.#....#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#........#......#........#..." +
        ////   "...#...........................G";
        //int n = 256, m = 256;
        //string mapStr = LabyrinthGenerator.Generate(n, m, wallChance: 0.10);

        //LabyrinthGenerator.PrintAsRows(mapStr,n,m);

        // 3) If you have a GridMap type, create it (stubbed here)
        //    If your GridMap constructor differs, adjust accordingly.
        GridMap gridmap = new GridMap(n, m, gridSize: 8);
        gridmap.MapFromStr(n, m, mapStr);
        gridmap.SetIsUsingOneGatePerEdge(true)
               .InitComponents()
               .InitChunks()
               .InitGates()
               .InitConnections(algo);


        GridMapV2 gridmapv2 = new GridMapV2(n, m, gridSize: 4);
        gridmapv2.MapFromStr(n, m, mapStr);
        gridmapv2.InitComponents();
        gridmapv2.SetIsUsingOneGatePerEdge(true)
                 .InitChunks()
                 .InitGatesV2()
                 .InitConnections()
                 .ConnectStartGate()
                 .ConnectGoalGate();



        //ChunkVisualizer.PrintChunksWithGates(gridmapv2);
        //ConnectionVisualizer.PrintConnections(gridmapv2);


        var pa = gridmapv2.GetGridPath(algo);

        //PrintPathAsAscii(gridmap, pa.path);
        Visualizers.AnimateAsAscii(gridmapv2, pa, delayMs: 80);

        //ChunkVisualizer.PrintChunksWithGates(gridmap);
        stopwatch.Start();
        FinalPath p = new();
        for (int i = 0; i < 100; i++) { // 4897 ms
            //Console.WriteLine($"i : {i}");
            p = gridmap.GetGridPath(algo);
            //FinalPath result = manager.Run();

        }
        stopwatch.Stop();
        Console.WriteLine($"Time taken grid: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
        var path = gridmap.GetGridPath(algo);

        //Visualizers.AnimateAsAscii(gridmap, path, delayMs: 80);
        //ConnectionVisualizer.PrintConnections(gridmap);
        // 4) Run solver through Manager
        var pmap = new PMap(n, m);
        pmap.MapFromStr(n, m, mapStr);
        var manager = new Manager(algo, pmap, gridmap);

        var nodes = pmap.ToNodes(pmap.cells);   // build the full graph once
        var startNode = nodes[pmap.start];
        var goalNode = nodes[pmap.goal];
        stopwatch.Restart();
        for (int i = 0; i < 100; i++) { // 31964 ms
            p = algo.FindGoal(startNode, goalNode);
        }

        stopwatch.Stop();
        Console.WriteLine($"Time taken simple: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");

        //// 5) ASCII printer: print the chosen shortest path
        ////PrintPathAsAscii(pmap, result.path);
        //
        //Visualizers.AnimateAsAscii(pmap, result, delayMs: 80);


        //// optionally pause at end
        //Console.WriteLine("Done. Press any key...");
        //Console.ReadKey();
    }

    
}