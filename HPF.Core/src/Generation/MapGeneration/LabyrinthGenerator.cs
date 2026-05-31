using System;
using System.Collections.Generic;

namespace HPF.model;


public class Labyrinth {

    public int N { get; }
    public int M { get; }

    public Node Start { get; set; }
    public Node Goal { get; set; }

    public Node[,] Cells { get; }
    private Random _random;
    
    public Labyrinth(int n, int m, bool isStartRestrictedToEdge = false) {
        N = n;
        M = m;

        _random = new Random();

        Cells = new Node[n, m];
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < m; j++) {
                if (Cells[i, j] == null) {
                    Cells[i, j] = new Node(
                        Pos: new Vector2(i, j),
                        Connections: []
                    );
                }
            }
        }

        ((int startRow, int startCol), (int goalRow, int goalCol)) =
            isStartRestrictedToEdge ? RandomAtEdge() : RandomStartAndGoal();

        Start = Cells[startRow, startCol];
        Goal = Cells[goalRow, goalCol];



    }

    private ((int startRow, int startCol), (int goalRow, int goalCol)) RandomAtEdge() {

        int r1 = _random.Next(1);
        int r2 = _random.Next(1);

        int startRow = (N - 1) * ((r1 == 0) ? 1 : 0);
        int startCol = (M - 1) * ((r2 == 0) ? 1 : 0);

        int goalRow = (N - 1) * ((r1 == 1) ? 1 : 0);
        int goalCol = (M - 1) * ((r2 == 1) ? 1 : 0);

        return ((startRow, startCol), (goalRow, goalCol));
    }

    private ((int startRow, int startCol), (int goalRow, int goalCol)) RandomStartAndGoal() {
        int startRow = 0;
        int startCol = 0;

        int goalRow = 0;
        int goalCol = 0;

        bool isValidCoordinates = false;
        while (!isValidCoordinates) {
            startRow = _random.Next(N);
            startCol = _random.Next(M);
            goalRow = _random.Next(N);
            goalCol = _random.Next(M);

            isValidCoordinates = !(startRow == goalRow &&
                                   startCol == goalCol);

        }
        return ((startRow, startCol), (goalRow, goalCol));
    }

    public string ToFlatString() {
        int rows = 2 * N - 1;
        int cols = 2 * M - 1;
        char[] grid = new char[rows * cols];

        Array.Fill(grid, '#');

        for (int row = 0; row < N; row++) {
            for (int col = 0; col < M; col++) {
                var cell = Cells[row, col];
                int gridRow = row * 2;
                int gridCol = col * 2;

                if (cell == Start)
                    grid[gridRow * cols + gridCol] = 'S';
                else if (cell == Goal)
                    grid[gridRow * cols + gridCol] = 'G';
                else
                    grid[gridRow * cols + gridCol] = '.';

                if (col + 1 < M && cell.Connections.Contains(Cells[row, col + 1]))
                    grid[gridRow * cols + (gridCol + 1)] = '.';

                if (row + 1 < N && cell.Connections.Contains(Cells[row + 1, col]))
                    grid[(gridRow + 1) * cols + gridCol] = '.';
            }
        }

        var sb = new System.Text.StringBuilder();
        for (int row = 0; row < rows; row++) {
            sb.Append(grid, row * cols, cols);
            // sb.Append("\n");
        }

        return sb.ToString();
    }

    public override string ToString() {
        var sb = new System.Text.StringBuilder();

        for (int row = 0; row < N; row++) {
            for (int col = 0; col < M; col++) {
                sb.Append('+');
                var cell = Cells[row, col];
                bool passageUp = row > 0 && cell.Connections.Contains(Cells[row - 1, col]);
                sb.Append(passageUp ? "  " : "--");
            }
            sb.AppendLine("+");

            for (int col = 0; col < M; col++) {
                var cell = Cells[row, col];
                bool passageLeft = col > 0 && cell.Connections.Contains(Cells[row, col - 1]);
                sb.Append(passageLeft ? " " : "|");

                if (cell == Start) sb.Append("S ");
                else if (cell == Goal) sb.Append("G ");
                else sb.Append("  ");
            }
            sb.AppendLine("|");
        }

        for (int col = 0; col < M; col++) {
            sb.Append("+--");
        }
        sb.AppendLine("+");

        return sb.ToString();
    }

}
public class LabyrinthGenerator {
    private readonly Random _random = new();


    public Labyrinth Generate(int n, int m, bool isStartRestrictedToEdge = false) {
        if (!(n >= 1 && m >= 1)) {
            throw new ArgumentException($"Labyrinth too small n : {n}, m : {m}");
        }
        var labyrinth = new Labyrinth(n, m, isStartRestrictedToEdge);
        GenerateLabyrinth(labyrinth);

        return labyrinth;
    }

    private void GenerateLabyrinth(Labyrinth labyrinth) {
        Stack<Node> stack = new Stack<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        var start = labyrinth.Start;

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0) {
            var current = stack.Peek();

            var unvisitedNeighbor = GetRandomUnvisitedNeighbor(current, labyrinth, visited);

            if (unvisitedNeighbor != null) {

                current.Connections.Add(unvisitedNeighbor);
                unvisitedNeighbor.Connections.Add(current);

                visited.Add(unvisitedNeighbor);
                stack.Push(unvisitedNeighbor);
            } else {

                stack.Pop();
            }
        }
    }

    private Node? GetRandomUnvisitedNeighbor(Node current, Labyrinth labyrinth, HashSet<Node> visited) {
        List<(int Row, int Col)> dirs = [(1, 0), (0, 1), (-1, 0), (0, -1)];
        Shuffle(dirs);

        foreach (var (Row, Col) in dirs) {
            int newRow = current.Pos.Row + Row;
            int newCol = current.Pos.Col + Col;

            if (newRow < 0 || newRow >= labyrinth.N) continue;
            if (newCol < 0 || newCol >= labyrinth.M) continue;

            var neighbor = labyrinth.Cells[newRow, newCol];
            if (neighbor != null && !visited.Contains(neighbor))
                return neighbor;
        }

        return null; // All neighbors visited → backtrack
    }
    // from https://stackoverflow.com/questions/273313/randomize-a-listt
    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = _random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}