
using System.Collections.Generic;
using Sudoku.Core;

namespace Sudoku.GeneticAlgorithmSolver
{
    /// <summary>
    /// Each type of chromosome for solving a sudoku is simply required to output a list of candidate sudokus
    /// </summary>
    public interface ISudokuChromosome
    {
        IList<GrilleSudoku> GetSudokus();
    }
}