using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Core;

namespace Sudoku.CSP
{
    public class CspSolver : ISudokuSolver
    {
        
        // FONCTION DE FOWARD CHECKING
        public List<List<int>> Foward_Checking(GrilleSudoku s)
        {
            List<List<int>> LesPoss = new List<List<int>>();

            for (int i = 0; i < 81; i++)
            {
                List<int> Poss = new List<int>();
                if (s.GetCellule(i/9,i%9) != 0)
                {
                    Poss.Add(10);
                }
                else
                {
                    foreach (int possibilities in s.GetPossibilities(i / 9, i % 9))
                    {
                        Poss.Add(possibilities);

                    }
                }
                LesPoss.Add(Poss);
            }
            return LesPoss;
        }
        

        // FONCTION POUR ARC CONSISTENCY CHECKING
        public int getNextMRV(List<List<int>> possibilites)
        {
            List<int> coord = new List<int>();
            Dictionary<int, int> LengthRemainValue = new Dictionary<int, int>();
            for (int i = 0; i < 81; i++)
            {
                LengthRemainValue.Add(i, possibilites[i].Count);
            }

            foreach (KeyValuePair<int, int> rv in LengthRemainValue.OrderBy(key => key.Value))
            {

                foreach (int poss in possibilites[rv.Key])
                {
                    if (poss != 10)
                    {
                        return rv.Key;
                    }

                }
            }
            return -1;
        }
        public void AC3(GrilleSudoku s, List<List<int>> possibilites)
        {
            bool test = true;
            int CaseCheck;
            while (test)
            {
                test = false;
                for (int i = 0; i < 81; i++)
                {
                    if (possibilites[i].Count == 1 && possibilites[i][0] != 10)
                        test = true;
                }
                if (test == true)
                {
                    CaseCheck = getNextMRV(possibilites);

                    //AC3 ETAPE : 1
                    //Nb Possibilites = 1 
                    if (possibilites[CaseCheck].Count == 1)
                    {
                        s.SetCell(CaseCheck / 9, CaseCheck % 9, possibilites[CaseCheck][0]);
                        DeletePossibilites(s, possibilites, CaseCheck, possibilites[CaseCheck][0]);
                    }
                }
            }
        }
        public void DeletePossibilites(GrilleSudoku s, List<List<int>> possibilites, int cellule, int valeur)
        {
            // supp la Valeur des Possibilités de la Cellule 
            possibilites[cellule].Remove(valeur);

            if (possibilites[cellule].Count == 0)
                possibilites[cellule].Add(10);

            // supp la Valeur des Possibilités des Voisins de la Cellule
            foreach (int line in Cell_Neighboor(s, cellule))
                possibilites[line].Remove(valeur);

        }
        
       
       // FONCTIONS UTILES
        public bool Is_Finished(List<List<int>> possibilites)
        {
            for (int i = 0; i < 81; i++)
            {
                if (possibilites[i].Count > 1)
                    return false;
            }
            return true;
        } 
        public bool Is_present(GrilleSudoku s, List<List<int>> possibilites,int poss, int position)
        {
            foreach(int voisin in Cell_Neighboor(s,position))
            {
                int ligne = voisin / 9;
                int colonne = voisin % 9;
                
                if(s.GetCellule(ligne,colonne) == poss)
                        return false;
            }
            return true;
        }
        public List<int> Cell_Neighboor(GrilleSudoku s, int cases)
        {
            bool test;
            List<int> ma_list = new List<int>();

            for (int i = 0; i < s.GetVoisins().Count; i++)
            {
                test = false;

                if (s.GetVoisins()[i].Contains(cases))
                {
                    foreach (int voisin in s.GetVoisins()[i])
                    {
                        if (!ma_list.Contains(voisin) && voisin != cases)
                        {
                            ma_list.Add(voisin);
                        }
                    }
                }
            }
            return ma_list;
        }


        // FONCTION POUR  BACKTRACKING
        public int Nombre_de_conflits(GrilleSudoku s, int cellule, int valeur, List<List<int>> possibilites)
        {
            int count = 0;
            foreach (int cell in Cell_Neighboor(s, cellule))
            {
                if (possibilites[cell].Count > 1 && possibilites[cell].Contains(valeur))
                    count = count + 1;
            }
            return count;
        }
        public List<int> order_domain_values(GrilleSudoku s, int cellule, List<List<int>> possibilites)
        {
            List<int> poss = new List<int>();
            if (possibilites[cellule].Count == 1)
            {
                poss.Add(possibilites[cellule][0]);
                return poss;
            }
            Dictionary<int, int> mon_dico = new Dictionary<int, int>();
            foreach(int key in possibilites[cellule])
                mon_dico.Add( key , Nombre_de_conflits(s,cellule,key,possibilites) );

            foreach (KeyValuePair<int, int> rv in mon_dico.OrderBy(key => key.Value))
            {
                poss.Add(rv.Key);
            }
            return poss;
        }
        public int select_unassigned_variable(List<int> assignment , GrilleSudoku s, List<List<int>> possibilites)
        {
            List<int> unassigned = new List<int>();
            List<int> tampon = new List<int>();

            for (int i=0;i<81;i++)
            {
                if (!assignment.Contains(i))
                    unassigned.Add(i);
            }
            Dictionary<int, int> mon_dico = new Dictionary<int, int>();
            foreach (int val in unassigned)
            {
                mon_dico.Add(val, possibilites[val].Count);
            }
            foreach (KeyValuePair<int, int> rv in mon_dico.OrderBy(key => key.Value))
                tampon.Add(rv.Key);
            return tampon[0];

        }
        public void assign(GrilleSudoku s, int cell, int value, List<int> assignment, List<List<int>> possibilites, List<List<int>> pruned)
        {

            assignment.Add(cell);
            s.SetCell(cell / 9, cell % 9, value);

            if (possibilites[cell].Contains(value))
            {
                possibilites[cell].Remove(value);
            }

            if (!pruned[cell].Contains(value))
            {
                pruned[cell].Add(value);
            }
                
            if (possibilites[cell].Count == 0)
                possibilites[cell].Add(10);

            // supp la Valeur des Possibilités des Voisins de la Cellule
            foreach (int neighboor_value in Cell_Neighboor(s, cell))
            {
                if(possibilites[neighboor_value].Contains(value))
                    possibilites[neighboor_value].Remove(value);
                if(!pruned[neighboor_value].Contains(value))
                    pruned[neighboor_value].Add(value);
            }
        }
        public void unassign(GrilleSudoku s, int cell, List<int> assignment, List<List<int>> possibilites, List<List<int>> pruned)
        {
            int val_cell = s.GetCellule(cell / 9, cell % 9);

            if (assignment.Contains(cell))
            {
                
                 if( !possibilites[cell].Contains(val_cell) )
                      possibilites[cell].Add(val_cell);
                    
                 
                  if (possibilites[cell].Contains(10))
                      possibilites[cell].Remove(10);
                 
                 assignment.Remove(cell);
                 s.SetCell(cell / 9, cell % 9, 0);
                 List<int> l = new List<int>();
                 pruned[cell] = l;
                
                foreach (int neighboor_value in Cell_Neighboor(s, cell))
                {
                    List<int> r = new List<int>();

                    if (!possibilites[neighboor_value].Contains(val_cell) )
                    {
                        if (Is_present(s, possibilites, val_cell, neighboor_value))
                        {
                            possibilites[neighboor_value].Add(val_cell);
                            //Console.WriteLine("Voisin : {0} Re add dans poss : {1}", neighboor_value, val_cell);
                        }

                    }

                    if (pruned[neighboor_value].Contains(val_cell))
                        pruned[neighboor_value].Remove(val_cell);
                }

            }
            else
            {
                Console.WriteLine("ERROOR 265 : UNSIGN A NON SIGN");
            }
        }
        public bool recursive_backtrack_algorithm(GrilleSudoku s, List<List<int>> possibilites, List<int> assignment, List<List<int>> pruned)
        {
            if (assignment.Count == 81)
            {
                Console.WriteLine("FINI !");
                return true;
            }
                
            // MRV
            int cellule = select_unassigned_variable(assignment, s, possibilites);
            // HEURISTIC
            foreach ( int value in order_domain_values(s,cellule,possibilites))
            {
                if (Is_present(s, possibilites, value, cellule))
                {
                    //Console.WriteLine("____");
                    //Console.WriteLine("Length : {0}", assignment.Count);
                    
                    
                    //foreach (int var in order_domain_values(s, cellule, possibilites))
                        //Console.WriteLine("Cellule : {0} Valeur set : {2}  Poss : {1}", cellule, var, value);
                    
                    assign(s, cellule, value, assignment, possibilites, pruned);
                       
                    if (recursive_backtrack_algorithm(s, possibilites, assignment, pruned))
                        return true;
                    else
                    {
                        //Console.WriteLine("CELLULE : {1} Unsign : {0} ",value,cellule);
                        unassign(s, cellule, assignment, possibilites, pruned);
                    }
                }  
            }

            //unassign(s, cellule, assignment, possibilites, pruned);
            return false;

        }

        


        // FONCTION SOLVE
        public void Solve(GrilleSudoku s)
        {
            // Foward Checking
            List<List<int>> possibilites = this.Foward_Checking(s);
            List<List<int>> pruned = new List<List<int>>();

            List<int> liste = new List<int>();
            for (int i = 0; i < 81; i++)
                pruned.Add(liste);

             // ACR CONSITENCY CHECKING
             AC3(s, possibilites);
             if (Is_Finished(possibilites))
                 Console.WriteLine("Résolue juste avec AC3  !!!");
              
             else
             {
                List<int> assignment = new List<int>();
                for(int i=0;i<81;i++)
                {
                    if (s.GetCellule( i/9 , i%9 ) != 0)
                        assignment.Add(i);
                }
                if ( recursive_backtrack_algorithm(s, possibilites, assignment, pruned) )
                    Console.WriteLine("Résolue avec Backtracking !!");
            }
        }

    }
}

