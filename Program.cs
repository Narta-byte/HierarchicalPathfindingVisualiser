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


using HPF.model;

public static class Program
{
    public static void Main()
    {
        // 1) 
        IAlgo algo = new BFS();

        // 2) Create a map (S=start, G=goal, #=wall, .=empty)
        //    NOTE: MapFromStr expects ALL rows concatenated into one string.
        int n = 7, m = 10;
        string mapStr =
            "S....#...." +
            ".##..#...." +
            ".#...#..#." +
            ".#.#.....#" +
            ".#.#.###.#" +
            ".#...#...G" +
            ".....#....";

        var pmap = new PMap(n, m);
        pmap.MapFromStr(n, m, mapStr);

        // 3) If you have a GridMap type, create it (stubbed here)
        //    If your GridMap constructor differs, adjust accordingly.
        GridMap gridmap = new GridMap(n, m, gridSize: 4);
        gridmap.MapFromStr(n,m,mapStr);
        gridmap.SetIsUsingOneGatePerEdge(true);
        gridmap.InitChunks();
        gridmap.InitGates();
        gridmap.InitConnections(algo);

        ChunkVisualizer.PrintChunksWithGates(gridmap);

        //// 4) Run solver through Manager
        //var manager = new Manager(algo, pmap, gridmap);
        //FinalPath result = manager.Run();

        //// 5) ASCII printer: print the chosen shortest path
        ////PrintPathAsAscii(pmap, result.path);
        //Visualizers.AnimateAsAscii(pmap, result, delayMs: 80);


        //// optionally pause at end
        //Console.WriteLine("Done. Press any key...");
        //Console.ReadKey();
    }

    private static void PrintPathAsAscii(PMap map, List<Vector2> path)
    {
        // Build a lookup for the path cells
        var pathSet = new HashSet<Vector2>(path);

        for (int r = 0; r < map.N; r++)
        {
            for (int c = 0; c < map.M; c++)
            {
                var coord = new Vector2(r, c);

                // Preserve S and G
                if (coord == map.Start()) { Console.Write('S'); continue; }
                if (coord == map.Goal) { Console.Write('G'); continue; }

                // Walls
                if (map.IsWall(r, c)) { Console.Write('#'); continue; }

                // Path cells
                if (pathSet.Contains(coord)) { Console.Write('*'); continue; }

                // Empty
                Console.Write('.');
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Path length: {path.Count}");
    }
}