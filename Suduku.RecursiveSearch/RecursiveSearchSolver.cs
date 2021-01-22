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
            for (int ligne = 0; ligne < Size; ligne++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (s.GetCellule(ligne, col) == 0) // empty
                    {
                        for (int value = 1; value <= 9; value++)
                        {
                            if (IsValid(s, ligne, col, value))
                            {
                                s.SetCell(ligne, col, value);

                                if (Resolve(s))
                                {
                                    return true;
                                }
                                else
                                {
                                    s.SetCell(ligne, col, 0);
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsValid(GrilleSudoku s, int ligne, int col, int value)
        {
            return IsLigneValid(s, ligne, value) &&
                   IsCarreValid(s, ligne, col, value) &&
                   IsColValid(s, col, value);
        }

        private bool IsLigneValid(GrilleSudoku s, int ligne, int value)
        {
            for (var col = 0; col < Size; col++)
            {
                if (s.GetCellule(ligne, col) == value)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsColValid(GrilleSudoku s, int col, int value)
        {
            for (var ligne = 0; ligne < Size; ligne++)
            {
                if (s.GetCellule(ligne, col) == value)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsCarreValid(GrilleSudoku s, int ligne, int col, int value)
        {
            var l = ligne - ligne % 3;
            var c = col - col % 3;

            for (var i = l; i < l + 3; i++)
            {
                for (var j = c; j < c + 3; j++)
                {
                    if (s.GetCellule(i, j) == value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}



