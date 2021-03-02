using System;
using System.Collections.Generic;
using System.Linq;  //enumerable varaible
using Google.OrTools.ConstraintSolver;
using Sudoku.Core;

namespace Sudoku.ORtools
{
    public class SolverORtools : ISudokuSolver
    {
        public void Solve(GrilleSudoku s)
        {
            //On déclare notre solveur avec contraintes
            Solver solver = new Solver("Sudoku");


            // Matrice 2 dimensions de notre sudoku
            int[,] sudokuMatrix = new int[9, 9];

            //Matrice vide de variables pour
            //IntVar[,] sudokuVariable = new IntVar[9, 9];
            IntVar[,] sudokuVariable = solver.MakeIntVarMatrix(9, 9, 1, 9, "sudokuVariable");

            //Matrice sudokuVariable aplatie
            IntVar[] sudokuResultat = sudokuVariable.Flatten();


            //On déclare le format de notre sudoku
            IEnumerable<int> sudokuSize = Enumerable.Range(0, 9);

            IEnumerable<int> boxSize = Enumerable.Range(0, 3);



            

            //On met le sudoku dans la matrice sudokuMatrix
            for (int row = 0; row <= 8; row++)
            {
                for (int column = 0; column <= 8; column++)
                {
                    sudokuMatrix[row, column] = s.GetCellule(row, column);
                }
            }



            //Créer et initialiser les variables

            for (int row = 0; row <= 8; row++)
            {
                for (int column = 0; column <= 8; column++)
                {
                    if (sudokuMatrix[row, column] > 0)  // Si valeur comprise entre 1 et 9, la case garde sa valeur
                                                        // Sinon la case reste une case variable
                    {
                        // On stock les valeurs déjà définies dans matrice sudokuVariable
                        solver.Add(sudokuVariable[row, column] == sudokuMatrix[row, column]);
                    }
                }
            }





                    // Déclaration des contraintes

                    foreach (int i in sudokuSize)
            {

                //rows
                //Chaque ligne à des chiffres différents
                solver.Add((from j in sudokuSize select sudokuVariable[i, j]).ToArray().AllDifferent());

                //columns
                //Chaque colonne à des chiffres différents 
                solver.Add((from j in sudokuSize select sudokuVariable[j, i]).ToArray().AllDifferent());

            }

            // regions
            //On veut que chaque région soit composé de chiffres différents
            foreach (int i in boxSize)
            {
                for (int j = 0; j <= 2; j++)
                {
                    solver.Add((from di in boxSize from dj in boxSize select sudokuVariable[i * 3 + di, j * 3 + dj]).ToArray().AllDifferent());
                }
            }






            //// On solutionne notre sudoku

            DecisionBuilder db = solver.MakePhase(sudokuResultat, Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);

            solver.NewSearch(db);


            while (solver.NextSolution())
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int column = 0; column < 9; column++)
                    {
                        Console.Write("{0} ", sudokuVariable[row, column].Value());
                        s.SetCell(row, column, Convert.ToInt32(sudokuVariable[row, column].Value()));

                    }
                    Console.WriteLine();
                }
                 Console.WriteLine();
            }

          
            // On affiche le résultat produit par le solveur ORtools
            Console.WriteLine("\nSolutions: {0}", solver.Solutions());
            Console.WriteLine("WallTime: {0}ms", solver.WallTime());
            Console.WriteLine("Failures: {0}", solver.Failures());
            Console.WriteLine("Branches: {0} ", solver.Branches());

            solver.EndSearch();


        }
    }
}