using System;
using System.Collections.Generic;

namespace HPF.model;


public class Labyrinth {
    public int N { get; }
    public int M { get; }

    public Node Start { get; set; }
    public Node Goal { get; set; }

    public Node[,] Cells { get; }

    public Labyrinth(int n, int m, Node start, Node goal) {
        N = n;
        M = m;
        Start = start;
        Goal = goal;

        Cells = new Node[n, m];

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
            //sb.Append('\n');
        }

        return sb.ToString();
    }

    public override string ToString() {
        var sb = new System.Text.StringBuilder();

        for (int row = 0; row < N; row++) {
            // Top border of this row
            for (int col = 0; col < M; col++) {
                sb.Append("+");
                var cell = Cells[row, col];
                // Check if there's a passage to the cell above
                bool passageUp = row > 0 && cell.Connections.Contains(Cells[row - 1, col]);
                sb.Append(passageUp ? "  " : "--");
            }
            sb.AppendLine("+");

            // Left border + cell content
            for (int col = 0; col < M; col++) {
                var cell = Cells[row, col];
                // Check if there's a passage to the cell to the left
                bool passageLeft = col > 0 && cell.Connections.Contains(Cells[row, col - 1]);
                sb.Append(passageLeft ? " " : "|");

                // Cell content
                if (cell == Start) sb.Append("S ");
                else if (cell == Goal) sb.Append("G ");
                else sb.Append("  ");
            }
            sb.AppendLine("|");
        }

        // Bottom border
        for (int col = 0; col < M; col++) {
            sb.Append("+--");
        }
        sb.AppendLine("+");

        return sb.ToString();
    }

}
public class LabyrinthGenerator {
    private readonly Random _random = new();


    public Labyrinth Generate(int n, int m) {
        if (!(n >= 1 && m >= 1)) {
            throw new ArgumentException($"Labyrinth too small n : {n}, m : {m}");
        }
        // Place random start and goal

        bool isValidCoordinates = false;
        int startRow = 0;
        int startCol = 0;

        int goalRow = 0;
        int goalCol = 0;


        while (!isValidCoordinates) { 
            startRow = _random.Next(n);
            startCol = _random.Next(m);
            goalRow = _random.Next(n);
            goalCol = _random.Next(m);

            isValidCoordinates = !(startRow == goalRow &&
                                   startCol == goalCol);

        }
        Node startNode = new Node(
            Pos: new Vector2(startRow, startCol),
            Connections: []
        );
        Node goalNode = new Node(
            Pos: new Vector2(goalRow, goalCol),
            Connections: []
        );
        var labyrinth = new Labyrinth(n, m, startNode, goalNode);

        for (int i = 0; i < n; i++) {
            for (int j = 0; j < m; j++) {
                if (labyrinth.Cells[i, j] == null) {
                    labyrinth.Cells[i, j] = new Node(
                        Pos: new Vector2(i, j),
                        Connections: []
                    );
                }
            }
        }
        labyrinth.Start = labyrinth.Cells[startRow, startCol];
        labyrinth.Goal = labyrinth.Cells[goalRow, goalCol];

        // run DFS
        GenerateLabyrinth(labyrinth);

        // ??

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


    private void CreateConnections(Node current, Labyrinth labyrinth, HashSet<Node> visited) {
        List<(int Row, int Col)> dirs = [(1, 0), (0, 1), (-1, 0), (0, -1)];
        Shuffle(dirs);

        foreach (var (Row, Col) in dirs) {
            int newRow = current.Pos.Row + Row;
            int newCol = current.Pos.Col + Col;

            if (newRow >= labyrinth.N || newRow < 0) continue;
            if (newCol >= labyrinth.M || newCol < 0) continue;

            var neighbor = labyrinth.Cells[newRow, newCol];
            if (neighbor == null || visited.Contains(neighbor)) continue;

            // Bidirectional connection
            current.Connections.Add(neighbor);
            neighbor.Connections.Add(current);
        }
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
