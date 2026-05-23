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
        IAlgo aStar = new AStar();

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

        //int n = 8, m = 16;
        //string mapStr =
        //    "S..#........#..." +
        //    "...#........#..." +
        //    "...#........#..." +
        //    "...#........####" +
        //    "...#............" +
        //    "...#...........G" +
        //    "................" +
        //    "...#............";
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

        //string mapStr =
        //   ".S.#...#....#......#........#..." +
        //   "...#.#.#....#......#........#..." +
        //   "...#.#.#....#......#........#..." +
        //   ".....#.#....#......#........#..." +
        //   "...###.#....#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#........#......#........#..." +
        //   "...#...........................G";
        int n = 32, m = 32;
        //string mapStr = NoisyMapGenerator.Generate(n, m, wallChance: 0.30);

        //LabyrinthGenerator.PrintAsRows(mapStr,n,m);
        // Round up to nearest odd number
        var mapGen = new MapGenManager().SetMapSize(n,m);
        string labStr = mapGen.GenerateLabyrinth(true);
        //n = n * 2 - 1;
        //m = m * 2 - 1;
        Console.WriteLine(labStr);

        stopwatch.Start();

        
        GridMap gridmapv2 = 
            new GridMapBuilder(n,m,gridSize:4)
                .WithMap(labStr)
                .WithOneGatePerEdge(false)
                .Build();


        stopwatch.Stop();
        Console.WriteLine($"Precomputing Time : {stopwatch.ElapsedMilliseconds} ms");

        //ChunkVisualizer.PrintChunksWithGates(gridmapv2);
        //ConnectionVisualizer.PrintConnections(gridmapv2);

        //PrintPathAsAscii(gridmap, pa.path);
        //var path = gridmapv2.GetGridPath(aStar);
        //Visualizers.AnimateAsAscii(gridmapv2, path, delayMs: 80);

        //ChunkVisualizer.PrintChunksWithGates(gridmapv2);

        FinalPath p = new();
        for (int i = 0; i < 3; i++) {
            p = gridmapv2.GetGridPath(algo);

        }
        stopwatch.Stop();
        Console.WriteLine($"Time taken gridv2: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
        stopwatch.Restart();
        p = new();
        for (int i = 0; i < 3; i++) {
            p = gridmapv2.GetGridPath(aStar);
            //FinalPath result = manager.Run();

        }
        stopwatch.Stop();
        Console.WriteLine($"Time taken gridv2 with Astar: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");

        //Visualizers.AnimateAsAscii(gridmap, path, delayMs: 80);
        //ConnectionVisualizer.PrintConnections(gridmap);
        var pmap = new PMap(n, m);
        pmap.MapFromStr(n, m, labStr);
        var manager = new Manager(algo, pmap);

        var nodes = pmap.ToNodes(pmap.cells);   // build the full graph once
        var startNode = nodes[pmap.start ?? new Vector2(0,0)];
        var goalNode = nodes[pmap.goal ?? new Vector2(0, 0)];
        stopwatch.Restart();

        //Visualizers.AnimateAsAscii(pmap,algo.FindGoal(startNode, goalNode), delayMs: 80);

        for (int i = 0; i < 3; i++) { 
            p = algo.FindGoal(startNode, goalNode);
        }

        stopwatch.Stop();
        Console.WriteLine($"Time taken simple: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
        stopwatch.Restart();
        for (int i = 0; i < 3; i++) {
            p = aStar.FindGoal(startNode, goalNode);
        }

        stopwatch.Stop();
        Console.WriteLine($"Time taken simple Astar: {stopwatch.ElapsedMilliseconds} ms. Path length : {p.nodes.Count}");
    }

    
}