﻿using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using Sudoku.Core;

namespace Sudoku.GeneticAlgorithmSolver
{
    /// <summary>
    /// This more elaborated chromosome manipulates rows instead of cells, and each of its 9 gene holds an integer for the index of the row's permutation amongst all that respect the target mask.
    /// Permutations are computed once when a new Sudoku is encountered, and stored in a static dictionary for further reference.
    /// </summary>
    public class SudokuPermutationsChromosome : ChromosomeBase, ISudokuChromosome
    {
        /// <summary>
        /// The target Sudoku mask to solve
        /// </summary>
        protected readonly GrilleSudoku TargetSudoku;

        /// <summary>
        /// The list of row permutations accounting for the mask
        /// </summary>
        protected readonly IList<IList<IList<int>>> TargetRowsPermutations;

        /// <summary>
        /// This constructor assumes no mask
        /// </summary>
        public SudokuPermutationsChromosome() : this(null)
        {
        }

        /// <summary>
        /// Constructor with a mask sudoku to solve, assuming a length of 9 genes
        /// </summary>
        /// <param name="targetCore.Sudoku">the target sudoku to solve</param>
        public SudokuPermutationsChromosome(GrilleSudoku targetSudoku) : this(targetSudoku, 9)
        {

        }

        /// <summary>
        /// Constructor with a mask and a number of genes
        /// </summary>
        /// <param name="targetCore.Sudoku">the target sudoku to solve</param>
        /// <param name="length">the number of genes</param>
        public SudokuPermutationsChromosome(GrilleSudoku targetSudoku, int length) : base(length)
        {
            TargetSudoku = targetSudoku;
            TargetRowsPermutations = GetRowsPermutations(TargetSudoku);
            for (int i = 0; i < Length; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
        }

        /// <summary>
        /// generates a chromosome gene from its index containing a random row permutation
        /// amongst those respecting the target mask. 
        /// </summary>
        /// <param name="geneIndex">the index for the gene</param>
        /// <returns>a gene generated for the index</returns>
        public override Gene GenerateGene(int geneIndex)
        {

            var rnd = RandomizationProvider.Current;
            int permIdx;
            do
            {
                //we randomize amongst the permutations that account for the target mask.
                permIdx = rnd.GetInt(0, TargetRowsPermutations[geneIndex].Count);
                var perm = GetPermutation(geneIndex, permIdx);

                //We pick a random previous row to check compatibility with
                if (geneIndex == 0)
                {
                    //we skip first row
                    break;
                }
                var checkRowIdx = rnd.GetInt(0, geneIndex);
                var checkPerm = GetPermutation(checkRowIdx);
                if (Range9.All(i => perm[i] != checkPerm[i]))
                {
                    break;
                }

            } while (true);



            return new Gene(permIdx);
        }

        public override IChromosome CreateNew()
        {
            var toReturn = new SudokuPermutationsChromosome(TargetSudoku);
            return toReturn;
        }


        /// <summary>
        /// builds a single Sudoku from the given row permutation genes
        /// </summary>
        /// <returns>a list with the single Sudoku built from the genes</returns>
        public virtual IList<GrilleSudoku> GetSudokus()
        {
            var listInt = new List<int>(81);
            for (int i = 0; i < 9; i++)
            {
                var perm = GetPermutation(i);
                listInt.AddRange(perm);
            }
            var sudoku = new GrilleSudoku(listInt);
            return new List<GrilleSudoku>(new[] { sudoku });
        }

        /// <summary>
        /// Gets the permutation to apply from the index of the row concerned
        /// </summary>
        /// <param name="rowIndex">the index of the row to permute</param>
        /// <returns>the index of the permutation to apply</returns>
        protected virtual List<int> GetPermutation(int rowIndex)
        {
            int permIDx = GetPermutationIndex(rowIndex);
            return GetPermutation(rowIndex, permIDx);
        }

        protected virtual List<int> GetPermutation(int rowIndex, int permIDx)
        {
          
            // we use a modulo operator in case the gene was swapped:
            // It may contain a number higher than the number of available permutations. 
            var perm = TargetRowsPermutations[rowIndex][permIDx % TargetRowsPermutations[rowIndex].Count].ToList();
            return perm;
        }

        /// <summary>
        /// Gets the permutation to apply from the index of the row concerned
        /// </summary>
        /// <param name="rowIndex">the index of the row to permute</param>
        /// <returns>the index of the permutation to apply</returns>
        protected virtual int GetPermutationIndex(int rowIndex)
        {
            return (int)GetGene(rowIndex).Value;
        }


        /// <summary>
        /// This method computes for each row the list of digit permutations that respect the target mask, that is the list of valid rows discarding columns and boxes
        /// </summary>
        /// <param name="Core.Sudoku">the target sudoku to account for</param>
        /// <returns>the list of permutations available</returns>
        public IList<IList<IList<int>>> GetRowsPermutations(GrilleSudoku objSudoku)
        {
            if (objSudoku == null)
            {
                return UnfilteredPermutations;
            }

            // we store permutations to compute them once only for each target Sudoku
            if (!_rowsPermutations.TryGetValue(objSudoku, out var toReturn))
            {
                // Since this is a static member we use a lock to prevent parallelism.
                // This should be computed once only.
                lock (_rowsPermutations)
                {
                    if (!_rowsPermutations.TryGetValue(objSudoku, out toReturn))
                    {
                        toReturn = GetRowsPermutationsUncached(objSudoku);
                        _rowsPermutations[objSudoku] = toReturn;
                    }
                }
            }
            return toReturn;
        }

        private IList<IList<IList<int>>> GetRowsPermutationsUncached(GrilleSudoku objSudoku)
        {
           var toReturn = new List<IList<IList<int>>>(9);
            for (int i = 0; i < 9; i++)
            {
                var tempList = new List<IList<int>>();
                foreach (var perm in AllPermutations)
                {
                    // Permutation should match current mask row numbers, and have numbers different that other mask rows
                    if (!Range9.Any(rowIdx=> Range9.Any(j => objSudoku.GetCellule(rowIdx, j) > 0
                                                && ((rowIdx == i && perm[j] != objSudoku.GetCellule(rowIdx, j))||(rowIdx!=i && perm[j] == objSudoku.GetCellule(rowIdx, j))))))
                    {
                        tempList.Add(perm);
                    }
                }
                toReturn.Add(tempList);
            }

            return toReturn;
        }



        /// <summary>
        /// Produces 9 copies of the complete list of permutations
        /// </summary>
        public static IList<IList<IList<int>>> UnfilteredPermutations
        {
            get
            {
                if (!_unfilteredPermutations.Any())
                {
                    lock (_unfilteredPermutations)
                    {
                        if (!_unfilteredPermutations.Any())
                        {
                            _unfilteredPermutations = Range9.Select(i => AllPermutations).ToList();
                        }
                    }
                }
                return _unfilteredPermutations;
            }
        }

        /// <summary>
        /// Builds the complete list permutations for {1,2,3,4,5,6,7,8,9}
        /// </summary>
        public static IList<IList<int>> AllPermutations
        {
            get
            {
                if (!_allPermutations.Any())
                {
                    lock (_allPermutations)
                    {
                        if (!_allPermutations.Any())
                        {
                            _allPermutations = GetPermutations(Enumerable.Range(1, 9), 9);
                        }
                    }
                }
                return _allPermutations;
            }
        }

        /// <summary>
        /// The list of compatible permutations for a given Sudoku is stored in a static member for fast retrieval
        /// </summary>
        private static readonly IDictionary<GrilleSudoku, IList<IList<IList<int>>>> _rowsPermutations = new Dictionary<GrilleSudoku, IList<IList<IList<int>>>>();

        /// <summary>
        /// The list of row indexes is used many times and thus stored for quicker access.
        /// </summary>
        private static readonly List<int> Range9 = Enumerable.Range(0, 9).ToList();

        /// <summary>
        /// The complete list of unfiltered permutations is stored for quicker access
        /// </summary>
        private static IList<IList<int>> _allPermutations = (IList<IList<int>>) new List<IList<int>>();
        private static IList<IList<IList<int>>> _unfilteredPermutations = (IList<IList<IList<int>>>) new List<IList<IList<int>>>();

        /// <summary>
        /// Computes all possible permutation for a given set
        /// </summary>
        /// <typeparam name="T">the type of elements the set contains</typeparam>
        /// <param name="list">the list of elements to use in permutations</param>
        /// <param name="length">the size of the resulting list with permuted elements</param>
        /// <returns>a list of all permutations for given size as lists of elements.</returns>
        static IList<IList<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return  list.Select(t => (IList<T>) (new T[] { t }.ToList())).ToList();

         var enumeratedList = list.ToList();
            return (IList<IList<T>>) GetPermutations(enumeratedList, length - 1)
              .SelectMany(t => enumeratedList.Where(e => !t.Contains(e)),
                (t1, t2) => (IList<T>) t1.Concat(new T[] { t2 }).ToList()).ToList();
        }



    }
}