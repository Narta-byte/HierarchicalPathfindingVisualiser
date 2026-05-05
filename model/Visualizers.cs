using System;
using System.Collections.Generic;
using System.Threading;
using HPF.model;

public static class Visualizers
{
    public static void AnimateAsAscii(PMap map, FinalPath finalPath, int delayMs = 120)
    {
        // Accumulate what we've processed so far
        var visited = new HashSet<Vector2>();
        var path = new HashSet<Vector2>();

        Console.CursorVisible = false;

        int lastLines = map.N + 2; // grid + 2 status lines

        // initial draw (empty)
        Console.SetCursorPosition(0, 0);
        PrintFrame(map, visited, path, path.Count, -1, 0);

        for (int i = 0; i < finalPath.animationSteps.Count; i++)
        {
            var step = finalPath.animationSteps[i];

            if (step.isPath)
                path.Add(step.pos);
            else if (step.isVisited)
                visited.Add(step.pos);

            Console.SetCursorPosition(0, 0);
            PrintFrame(map, visited, path, path.Count, i, finalPath.animationSteps.Count);

            Thread.Sleep(delayMs);
        }

        Console.CursorVisible = true;
        Console.SetCursorPosition(0, map.N + 2);
        Console.WriteLine("Done. Press any key...");
        Console.ReadKey();
    }

    private static void PrintFrame(PMap map, HashSet<Vector2> visited, HashSet<Vector2> path,
                                    int pathLen, int stepIndex, int totalSteps)
    {
        for (int r = 0; r < map.N; r++)
        {
            for (int c = 0; c < map.M; c++)
            {
                var coord = new Vector2(r, c);

                if (coord == map.Start()) { Console.Write('S'); continue; }
                if (coord == map.Goal) { Console.Write('G'); continue; }

                if (map.IsWall(r, c)) { Console.Write('#'); continue; }

                if (path.Contains(coord)) { Console.Write('*'); continue; }
                if (visited.Contains(coord)) { Console.Write('o'); continue; }

                Console.Write('.');
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Step: {stepIndex}/{totalSteps}   PathLen: {pathLen}");
        Console.WriteLine("Legend: S,G start/goal | # wall | o visited | * final path");
    }
}
