using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Core;

namespace Sudoku.CSP
{
    public class CspSolver : ISudokuSolver
    {
        // Initialise la liste des possibilités de chaques cellules 
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
        //Recupère les lignes,colonne,carré voisins de la case
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
                        if(!ma_list.Contains(voisin) && voisin!=cases)
                        {
                            ma_list.Add(voisin);
                        }
                    }
                }
            }
            return ma_list;
        }
        // Get la plus petite valeure possibles restantes
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
        public List<int> MRV(List<List<int>> possibilites)
        {
            List<int> ListMRV = new List<int>();
            Dictionary<int, int> LengthRemainValue = new Dictionary<int, int>();
            for (int i = 0; i < 81; i++)
            {
                LengthRemainValue.Add(i, possibilites[i].Count);
            }

            foreach (KeyValuePair<int, int> rv in LengthRemainValue.OrderBy(key => key.Value))
            {
                   ListMRV.Add(rv.Key);
            }
            return ListMRV;
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
                        s.SetCell(CaseCheck/9,CaseCheck%9, possibilites[CaseCheck][0]);
                        DeletePossibilites(s, possibilites, CaseCheck, possibilites[CaseCheck][0]);
                    }
                }
            }
        }
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
                
                if(s.GetCellule(ligne,colonne)==poss)
                        return false;
            }
            return true;
        }



        public bool BackTracking(GrilleSudoku S, List<int> position, List<List<int>> possibilites,int val)
        {
            if (val == 81)
                return true;
            
            int ligne = position[val] / 9;
            int colonne = position[val] % 9;

            if (S.GetCellule(ligne, colonne) != 0)
                return BackTracking(S, position, possibilites,val+1);

            
            foreach (int poss in possibilites[position[val]])
            {
                if (Is_present(S, possibilites, poss, position[val]))
                {

                    //Console.WriteLine("Position :{0} , Possibilitée : {1}", position[val],poss);
                    S.SetCell(ligne, colonne, poss);

                    if (BackTracking(S, position, possibilites,val+1))
                        return true;
                }
            }

            S.SetCell(ligne, colonne, 0);
            return false ;
        }



        public void Solve(GrilleSudoku s)
        {
            // Foward Checking
            List<List<int>> possibilites = this.Foward_Checking(s);
            
            List<int> position = MRV(possibilites);
            AC3(s, possibilites);
            if (Is_Finished(possibilites))
            {
                Console.WriteLine("Résolue juste avec AC3  !!!");
            }
            else
            {
                Console.WriteLine("Backtracking...");
                if (BackTracking(s,position, possibilites,0))
                    Console.WriteLine("Résolue avec BackTracking");
            }
          
            
        }

    }
}

