using System;
using System.Collections.Generic;
using Sudoku.Core;

namespace Sudoku.GeneticAlgorithmSolver
{
    public class CombinatorialEvolutionSolver : ISudokuSolver
    {
        public Core.Sudoku Solve(Core.Sudoku s)
        {

            var toReturn = SolveCombinatorial(s);


            return toReturn;
        }


       


        public Core.Sudoku SolveCombinatorial(Core.Sudoku s)
        {

            var sudokuTab = (Core.Sudoku)s.Clone();
            Console.WriteLine("Begin solving Sudoku using combinatorial evolution");
            Console.WriteLine("The Sudoku is:");

            var sudoku = CombinatorialSudoku.Convert(sudokuTab);

            const int numOrganisms = 200;
            const int maxEpochs = 5000;
            const int maxRestarts = 40;
            Console.WriteLine($"Setting numOrganisms: {numOrganisms}");
            Console.WriteLine($"Setting maxEpochs: {maxEpochs}");
            Console.WriteLine($"Setting maxRestarts: {maxRestarts}");

            var solver = new CombinatorialSudokuSolver();
            var solvedSudoku = solver.Solve(sudoku, numOrganisms, maxEpochs, maxRestarts);

            Console.WriteLine(solvedSudoku.Error == 0 ? "Success" : "Did not find optimal solution");
            Console.WriteLine("End Sudoku using combinatorial evolution");
            var solution = ConvertSolution(solvedSudoku);

            return solution;

        }



        public Core.Sudoku ConvertSolution(CombinatorialSudoku sudoku)
        {

            var list = new List<int> { };
            for (int row = 1; row <= 9; row++)
            {
                for (int column = 1; column <= 9; column++)
                {
                    list.Add(sudoku.CellValues[row - 1, column - 1]);
                }
            }
            var output = new Core.Sudoku(list);
            return output;

        }
    }
}