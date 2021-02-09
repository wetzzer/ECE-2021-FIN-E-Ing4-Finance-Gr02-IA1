﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku.TechniquesHumaines
{
    internal sealed class Region : IEnumerable<Cell>
    {
        public readonly Cell[] _cells;

        public Cell this[int index] => _cells[index];

        public Region(Cell[] cells)
        {
            _cells = (Cell[])cells.Clone();
        }

        public IEnumerable<Cell> GetCellsWithCandidate(int candidate)
        {
            return _cells.Where(c => c.Candidates.Contains(candidate));
        }
        public IEnumerable<Cell> GetCellsWithCandidates(params int[] candidates)
        {
            return _cells.Where(c => c.Candidates.ContainsAll(candidates));
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            return ((IEnumerable<Cell>)_cells).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cells.GetEnumerator();
        }
    }
}
