using Sudoku.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoku.TechniquesHumainesExploration
{
    public class TechniquesHumainesExplorationSolver : ISudokuSolver
    {
        public void Solve(GrilleSudoku s)
        {
            bool solved = false;
            bool full = false; // If this is true after a segment, the puzzle is solved and we can break
            bool deadEnd = false;

            Puzzle puzzle = new Puzzle();

            List allCells = null;
            Stack exploredCellValues = null;

            puzzle.RefreshCandidates();
            do
            {
                deadEnd = false;

                // First we do human inference
                do
                {
                    full = true;

                    bool changed = false;
                    // Check for naked singles or a completed puzzle
                    for (int x = 0; x < 9; x++)
                    {
                        for (int y = 0; y < 9; y++)
                        {
                            Cell cell = puzzle[x, y];
                            if (cell.Value == 0)
                            {
                                full = false;
                                // Check for naked singles
                                int[] a = cell.Candidates.ToArray(); // Copy
                                if (a.Length == 1)
                                {
                                    cell.Set(a[0]);
                                    changed = true;
                                }
                            }
                        }
                    }
                    // Solved or failed to solve
                    if (full || (!changed && !RunTechnique(puzzle)))
                    {
                        break;
                    }
                } while (true);

                full = puzzle.Rows.All(row => row.Cells.All(c => c.Value != 0));

                // If puzzle isn't full, we do exploration
                if (!full)
                {
                    // Les Sudokus les plus difficiles ne peuvent pas être résolus avec un stylo bille, c'est à dire en inférence pure.
                    // Il va falloir lacher le stylo bille et prendre le crayon à papier et la gomme pour commencer une exploration fondée sur des hypothèses avec possible retour en arrière
                    if (allCells == null)
                    {
                        allCells = puzzle.Rows.SelectMany((row, rowIdx) => row.Cells).ToList();
                        exploredCellValues = new Stack();
                    }
                    //puzzle.RefreshCandidates();

                    // Pour accélérer l'exploration et éviter de traverser la feuille en gommant trop souvent, on va utiliser les heuristiques des problèmes à satisfaction de contraintes
                    // cf. les slides et le problème du "coffre de voiture" abordé en cours

                    //heuristique MRV
                    var minCandidates = allCells.Min(cell => cell.Candidates.Count > 0 ? cell.Candidates.Count : int.MaxValue);

                    if (minCandidates != int.MaxValue)
                    {
                        // Utilisation de l'heuristique Deg: de celles qui ont le moins de candidats à égalité, on choisi celle la plus contraignante, celle qui a le plus de voisins (on pourrait faire mieux avec le nombre de candidats en commun avec ses voisins)
                        var candidateCells = allCells.Where(cell => cell.Candidates.Count == minCandidates);
                        //var degrees = candidateCells.Select(candidateCell => new {Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Aggregate(0, (sum, neighbour) => sum + neighbour.Candidates.Count) });
                        var degrees = candidateCells.Select(candidateCell => new { Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Count(c => c.Value == 0) }).ToList();
                        //var targetCell = allCells.First(cell => cell.Candidates.Count == minCandidates);
                        var maxDegree = degrees.Max(deg1 => deg1.Degree);
                        var targetCell = degrees.First(deg => deg.Degree == maxDegree).Cell;

                        //dernière exploration pour ne pas se mélanger les pinceaux

                        BackTrackingState currentlyExploredCellValues;
                        if (exploredCellValues.Count == 0 || !exploredCellValues.Peek().Cell.Equals(targetCell))
                        {
                            currentlyExploredCellValues = new BackTrackingState() { Board = puzzle.GetBoard(), Cell = targetCell, ExploredValues = new List() };
                            exploredCellValues.Push(currentlyExploredCellValues);
                        }
                        else
                        {
                            currentlyExploredCellValues = exploredCellValues.Peek();
                        }


                        //utilisation de l'heuristique LCV: on choisi la valeur la moins contraignante pour les voisins
                        var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
                        var neighbourood = targetCell.GetCellsVisible();
                        var valueConstraints = candidateValues.Select(v => new
                        {
                            Value = v,
                            ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
                        }).ToList();
                        var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
                        var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value;
                        currentlyExploredCellValues.ExploredValues.Add(exploredValue);
                        targetCell.Set(exploredValue);
                        //targetCell.Set(exploredValue, true);

                    }
                    else
                    {
                        //Plus de candidats possibles, on atteint un cul-de-sac
                        if (puzzle.IsValid())
                        {
                            solved = true;
                        }
                        else
                        {
                            deadEnd = true;
                        }


                        //deadEnd = true;
                    }
                }
                else
                {
                    //If puzzle is full, it's either solved or a deadend
                    if (puzzle.IsValid())
                    {
                        solved = true;
                    }
                    else
                    {
                        deadEnd = true;
                    }
                }


                if (deadEnd)
                {
                    //On se retrouve bloqué, il faut gommer et tenter d'autres hypothèses
                    BackTrackingState currentlyExploredCellValues = exploredCellValues.Peek();
                    //On annule la dernière assignation
                    currentlyExploredCellValues.Backtrack(puzzle);
                    var targetCell = currentlyExploredCellValues.Cell;
                    //targetCell.Set(0, true);
                    while (targetCell.Candidates.All(i => currentlyExploredCellValues.ExploredValues.Contains(i)))
                    {
                        //on a testé toutes les valeurs possibles, On est à un cul de sac, il faut revenir en arrière
                        exploredCellValues.Pop();
                        if (exploredCellValues.Count == 0)
                        {
                            Debug.WriteLine("bug in the algorithm techniques humaines");
                        }
                        currentlyExploredCellValues = exploredCellValues.Peek();
                        //On annule la dernière assignation
                        currentlyExploredCellValues.Backtrack(puzzle);
                        targetCell = currentlyExploredCellValues.Cell;
                        //targetCell.Set(0, true);
                    }
                    // D'autres valeurs sont possible pour la cellule courante, on les tente
                    //utilisation de l'heuristique LCV
                    var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
                    var neighbourood = targetCell.GetCellsVisible();
                    var valueConstraints = candidateValues.Select(v => new
                    {
                        Value = v,
                        ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
                    }).ToList();
                    var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
                    var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value;
                    currentlyExploredCellValues.ExploredValues.Add(exploredValue);
                    targetCell.Set(exploredValue);
                }


            } while (!solved);



            s = transformationToSudoku(puzzle);

        }
    }
}
