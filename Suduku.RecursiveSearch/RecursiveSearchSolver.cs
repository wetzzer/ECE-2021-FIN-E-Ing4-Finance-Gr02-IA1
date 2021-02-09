using System;
using System.Collections.Generic;
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

        public bool Resolve(GrilleSudoku s)  // La fonction solve va pemettre de choisir la solution valide satsifaisant les contraintes de sodoku
        {
            for (int ligne = 0; ligne < Size; ligne++)  // parcourt les lignes
            {
                for (int col = 0; col < Size; col++)  // parcourt les colonnes 
                {
                    // Ce double for parcourt chaque colonne pour une ligne donnée
                    if (s.GetCellule(ligne, col) == 0) // empty: si la cellule est vide alors on va rentrer dans la boucle for
                    {
                        for (int value = 1; value <= 9; value++) // qui va tester les valeur de 1 à 9
                        {
                            if (IsValid(s, ligne, col, value)) // Si la valeur est valide avec les conditions de validité qu'on va définirapres dans le code
                            {

                                s.SetCell(ligne, col, value); // Alors on remplie la case vide avec la valeur valide

                                if (Resolve(s))  // Et on appelle la fonction elle-meme pour la récursivité : Principe de backtracking
                                {
                                    return true; // On on valide la solution
                                }
                                else
                                {
                                    s.SetCell(ligne, col, 0);  // Sinon on réinitialise la valeur à 0
                                }
                            }
                        }

                        return false; // On refait le même processus
                    }
                }
            }

            return true; // Jusqu'à ce qu'on trouve une solution valide et on la garde
        }

        private bool IsValid(GrilleSudoku s, int ligne, int col, int value)
        {
            return IsLigneValid(s, ligne, value) &&
                   IsCarreValid(s, ligne, col, value) &&
                   IsColValid(s, col, value);
        }


        private bool IsLigneValid(GrilleSudoku s, int ligne, int value)
        //Vérifie que chaque nombre n'apparait qu'une fois sur la ligne
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
        //Vérifie que chaque nombre n'apparait qu'une fois sur la colonne
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
        //Vérifie que chaque valeur n'apparait qu'une fois dans le carré 
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


