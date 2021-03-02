using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Sudoku.Core;

namespace Sudoku.Benchmark
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Benchmarking GrilleSudoku Solvers");

            while (true)
            {
                try
                {
                    ConsoleColor foreground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine("Select Mode: \n1-Single Solver Test, \n2-Complete Benchmark (10 s max per sudoku), \n3-Complete Benchmark (5 mn max per GrilleSudoku), \n4-Exit program");
                    var strMode = Console.ReadLine();
                    int.TryParse(strMode, out var intMode);
                    //Console.SetBufferSize(130, short.MaxValue - 100);
                    switch (intMode)
                    {
                        case 1:
                            SingleSolverTest();
                            break;
                        case 2:
                            //Init solvers
                            var temp = new BenchmarkSolvers();
                            BenchmarkRunner.Run<BenchmarkSolvers>();
                            break;
                        case 3:
                            //Init solvers
                            var temp2 = new BenchmarkSolvers();
                            BenchmarkRunner.Run<FiveMinutesBenchmarkSolvers>();
                            break;
                        default:
                            return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        private static void SingleSolverTest()
        {
            var solvers = Core.GrilleSudoku.GetSolvers();
            Console.WriteLine("Select difficulty: 1-Easy, 2-Medium, 3-Hard");
            var strDiff = Console.ReadLine();
            int.TryParse(strDiff, out var intDiff);
            SudokuDifficulty difficulty = SudokuDifficulty.Hard;
            switch (intDiff)
            {
                case 1:
                    difficulty = SudokuDifficulty.Easy;
                    break;
                case 2:
                    difficulty = SudokuDifficulty.Medium;
                    break;
                case 3:
                    difficulty = SudokuDifficulty.Hard;
                    break;
                default:
                    break;
            }
            //SudokuDifficulty difficulty = intDiff switch
            //{
            //    1 => SudokuDifficulty.Easy,
            //    2 => SudokuDifficulty.Medium,
            //    _ => SudokuDifficulty.Hard
            //};

            var sudokus = SudokuHelper.GetSudokus(difficulty);

            Console.WriteLine($"Choose a puzzle index between 1 and {sudokus.Count}");
            var strIdx = Console.ReadLine();
            int.TryParse(strIdx, out var intIdx);
            var targetSudoku = sudokus[intIdx-1];

            Console.WriteLine("Chosen Puzzle:");
            Console.WriteLine(targetSudoku.ToString());

            Console.WriteLine("Choose a solver:");
            for (int i = 0; i < solvers.Count(); i++)
            {
                Console.WriteLine($"{(i + 1).ToString(CultureInfo.InvariantCulture)} - {solvers[i].GetType().FullName}");
            }
            var strSolver = Console.ReadLine();
            int.TryParse(strSolver, out var intSolver);
            var solver = solvers[intSolver - 1];

            var cloneSudoku = targetSudoku.CloneSudoku();
            var sw = Stopwatch.StartNew();

            solver.Solve(cloneSudoku);

            var elapsed = sw.Elapsed;
            if (!cloneSudoku.IsValid(targetSudoku))
            {
                Console.WriteLine($"Invalid Solution : Solution has {cloneSudoku.NbErrors(targetSudoku)} errors");
                Console.WriteLine("Invalid solution:");
            }
            else
            {
                Console.WriteLine("Valid solution:");
            }
            
            Console.WriteLine(cloneSudoku.ToString());
            Console.WriteLine($"Time to solution: {elapsed.TotalMilliseconds} ms");

        }

    }
}
