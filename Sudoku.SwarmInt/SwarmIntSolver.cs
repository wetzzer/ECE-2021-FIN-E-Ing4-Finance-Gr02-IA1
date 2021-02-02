using System;
using Sudoku.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.SwarmInt
{
    public class SwarmIntSolver : ISudokuSolver
    {
        public const int SIZE = 9;
        public const int BLOCK_SIZE = 3;
        static Random rnd = new Random(0);

        public void Solve(GrilleSudoku s)
        {




            Console.WriteLine("Begin solving Sudoku");
            Console.WriteLine("The problem is: ");

            int[][] problem = new int[9][];
            
            DisplayMatrix(problem);
            int numOrganisms = 200;
            int maxEpochs = 5000;
            int maxRestarts = 20;

            int[][] soln = Solve(problem, numOrganisms,maxEpochs, maxRestarts);

            Console.WriteLine("Best solution found: ");
            for (int row = 0; row <= 8; row++)
            {​​
                    problem[row] = new int[9];
                for (int column = 0; column <= 8; column++)
                {​​
                        problem[row][column] = s.GetCellule(row, column);
                }​​
            }​​
            DisplayMatrix(soln);

            int err = Error(soln);
            if (err == 0)
                Console.WriteLine("Success \n");
            else
                Console.WriteLine("Did not find optimal solution \n");
            Console.WriteLine("End Sudoku demo");
            Console.ReadLine();

        }

        private int Error(int[][] soln)
        {
            get
                {
                return CountErrors(true) + CountErrors(false);

                int CountErrors(bool countByRow)
                {
                    var errors = 0;
                    for (var i = 0; i < SwarmIntSolver.SIZE; ++i)
                    {
                        var counts = new int[SwarmIntSolver.SIZE];
                        for (var j = 0; j < SwarmIntSolver.SIZE; ++j)
                        {
                            var cellValue = countByRow ? CellValues[i, j] : CellValues[j, i];
                            ++counts[cellValue - 1];
                        }

                        for (var k = 0; k < SwarmIntSolver.SIZE; ++k)
                        {
                            if (counts[k] == 0)
                                ++errors;
                        }
                    }

                    return errors;
                }
            }
        }

        public static int[][] Solve(int[][] problem,int numOrganisms, int maxEpochs, int maxRestarts)
        { }
        

        public static void DisplayMatrix(int[][] matrix) { }


        public static int[][] SolveEvo(int[][] problem, int numOrganisms, int maxEpochs)
        {
            int numWorker = (int)(numOrganisms * 0.90);
            int numExplorer = numOrganisms - numWorker;
            Organism[] hive = new Organism[numOrganisms];
            // Initialize each Organism
            int epoch = 0;
            while (epoch < maxEpochs)
            {
                for (int i = 0; i < numOrganisms; ++i)
                {
                    // Process each Organism
                }
                // Merge best worker with best explorer, increment epoch
            }
            return bestMatrix;
        }



        public static int[,] RandomMatrix(Random rnd, int[,] problem)
        {
            var result = DuplicateMatrix(problem);

            for (var block = 0; block < SIZE; ++block)
            {
                var corner = Corner(block);
                var values = Enumerable.Range(1, SIZE).ToList();

                for (var k = 0; k < values.Count; ++k)
                {
                    var ri = rnd.Next(k, values.Count);
                    var tmp = values[k];
                    values[k] = values[ri];
                    values[ri] = tmp;
                }

                var r = corner.row;
                var c = corner.column;
                for (var i = r; i < r + BLOCK_SIZE; ++i)
                {
                    for (var j = c; j < c + BLOCK_SIZE; ++j)
                    {
                        var value = problem[i, j];
                        if (value != 0)
                            values.Remove(value);
                    }
                }

                var pointer = 0;
                for (var i = r; i < r + BLOCK_SIZE; ++i)
                {
                    for (var j = c; j < c + BLOCK_SIZE; ++j)
                    {
                        if (result[i, j] != 0) continue;
                        var value = values[pointer];
                        result[i, j] = value;
                        ++pointer;
                    }
                }
            }

            return result;
        }


        public static (int row, int column) Corner(int block)
        {
            int r = -1, c = -1;

            if (block == 0 || block == 1 || block == 2)
                r = 0;
            else if (block == 3 || block == 4 || block == 5)
                r = 3;
            else if (block == 6 || block == 7 || block == 8)
                r = 6;

            if (block == 0 || block == 3 || block == 6)
                c = 0;
            else if (block == 1 || block == 4 || block == 7)
                c = 3;
            else if (block == 2 || block == 5 || block == 8)
                c = 6;

            return (r, c);
        }


        public static int Block(int r, int c)
        {
            if (r >= 0 && r <= 2 && c >= 0 && c <= 2)
                return 0;
            if (r >= 0 && r <= 2 && c >= 3 && c <= 5)
                return 1;
            if (r >= 0 && r <= 2 && c >= 6 && c <= 8)
                return 2;
            if (r >= 3 && r <= 5 && c >= 0 && c <= 2)
                return 3;
            if (r >= 3 && r <= 5 && c >= 3 && c <= 5)
                return 4;
            if (r >= 3 && r <= 5 && c >= 6 && c <= 8)
                return 5;
            if (r >= 6 && r <= 8 && c >= 0 && c <= 2)
                return 6;
            if (r >= 6 && r <= 8 && c >= 3 && c <= 5)
                return 7;
            if (r >= 6 && r <= 8 && c >= 6 && c <= 8)
                return 8;

            throw new Exception("Unable to find Block()");
        }


        public static int[,] NeighborMatrix(Random rnd, int[,] problem, int[,] matrix)
        {
            // pick a random 3x3 block,
            // pick two random cells in block
            // swap values
            var result = DuplicateMatrix(matrix);

            var block = rnd.Next(0, SIZE); // [0,8]
            var corner = Corner(block);
            var cells = new List<int[]>();
            for (var i = corner.row; i < corner.row + BLOCK_SIZE; ++i)
            {
                for (var j = corner.column; j < corner.column + BLOCK_SIZE; ++j)
                {
                    if (problem[i, j] == 0)
                        cells.Add(new[] { i, j });
                }
            }

            if (cells.Count < 2)
                throw new Exception($"Block {block} doesn't have two values to swap!");

            // pick two. suppose there are 4 possible cells 0,1,2,3
            var k1 = rnd.Next(0, cells.Count); // 0,1,2,3
            var inc = rnd.Next(1, cells.Count); // 1,2,3
            var k2 = (k1 + inc) % cells.Count;

            var r1 = cells[k1][0];
            var c1 = cells[k1][1];
            var r2 = cells[k2][0];
            var c2 = cells[k2][1];

            var tmp = result[r1, c1];
            result[r1, c1] = result[r2, c2];
            result[r2, c2] = tmp;

            return result;
        }

        public int[,] CellValues { get; }

        

        public static int[,] DuplicateMatrix(int[,] matrix)
        {
            var m = matrix.GetLength(0);
            var n = matrix.GetLength(1);
            var result = CreateMatrix(m, n);
            for (var i = 0; i < m; ++i)
                for (var j = 0; j < n; ++j)
                    result[i, j] = matrix[i, j];

            return result;
        }

        public static int[,] CreateMatrix(int m, int n)
        {
            var result = new int[m, n];
            return result;
        }


    } // Program

}