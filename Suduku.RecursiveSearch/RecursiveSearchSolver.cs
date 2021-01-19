using System;
using Sudoku.Core;

namespace Suduku.RecursiveSearch
{
    public class RecursiveSearchSolver : ISudokuSolver

    {
        private const int Size = 9;

        public void Solve(GrilleSudoku s)
        {
            Resolve(s);

        }

        public bool Resolve(GrilleSudoku s)
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (s.GetCellule(row, col) == 0) // empty
                    {
                        for (int guess = 1; guess <= 9; guess++)
                        {
                            if (IsValid(s, row, col, guess))
                            {
                                s.SetCell(row, col, guess);

                                if (Resolve(s))
                                {
                                    return true;
                                }
                                else
                                {
                                    s.SetCell(row, col, 0);
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsValid(GrilleSudoku s, int row, int col, int guess)
        {
            return IsRowValid(s,row, guess) &&
                   IsColValid(s,col, guess) &&
                   IsSquareValid(s,row, col, guess);
        }

        private bool IsRowValid(GrilleSudoku s, int row, int guess)
        {
            for (var col = 0; col < Size; col++)
            {
                if (s.GetCellule(row, col) == guess)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsColValid(GrilleSudoku s, int col, int guess)
        {
            for (var row = 0; row < Size; row++)
            {
                if (s.GetCellule(row, col) == guess)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSquareValid(GrilleSudoku s, int row, int col, int guess)
        {
            var r = row - row % 3;
            var c = col - col % 3;

            for (var i = r; i < r + 3; i++)
            {
                for (var j = c; j < c + 3; j++)
                {
                    if (s.GetCellule(i, j) == guess)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}



