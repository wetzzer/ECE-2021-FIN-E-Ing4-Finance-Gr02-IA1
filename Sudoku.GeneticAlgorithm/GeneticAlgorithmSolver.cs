using System;
using Sudoku.Core;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;

namespace Sudoku.GeneticAlgorithmSolver
{
    public class GeneticAlgorithmSolver: ISudokuSolver
    {

        static GeneticAlgorithmSolver()
        {
           var init = SudokuPermutationsChromosome.AllPermutations;
        }

        public void Solve(GrilleSudoku s)
        {

            var populationSize = 5000;
            IChromosome sudokuChromosome = new SudokuPermutationsChromosome(s);
            //IChromosome sudokuChromosome = new SudokuCellsChromosome(s);
            var fitnessThreshold = 0;
            //var generationNb = 50;
            var crossoverProbability = 0.75f;
            var mutationProbability = 0.2f;
            var fitness = new SudokuFitness(s);
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation();

            IChromosome bestIndividual;
            var solution = s;
            int nbErrors = int.MaxValue;

            
            do
            {
                var population = new Population(populationSize, populationSize, sudokuChromosome);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
                {
                    Termination = new OrTermination(new ITermination[]
                    {
                        new FitnessThresholdTermination(fitnessThreshold),
                        new FitnessStagnationTermination(10), 
                        //new GenerationNumberTermination(generationNb)
                        //new TimeEvolvingTermination(TimeSpan.FromSeconds(10)), 
                    }),
                    MutationProbability = mutationProbability,
                    CrossoverProbability = crossoverProbability,
                    OperatorsStrategy = new TplOperatorsStrategy(),

                };
                ga.GenerationRan+= delegate(object sender, EventArgs args)
                {
                    bestIndividual = (ga.Population.BestChromosome);
                    solution = ((ISudokuChromosome)bestIndividual).GetSudokus()[0];
                    nbErrors = solution.NbErrors(s);
                    Console.WriteLine($"Generation #{ga.GenerationsNumber}: best individual has {nbErrors} errors");
                };
                ga.Start();

                //bestIndividual = (ga.Population.BestChromosome);
                //solution = ((ISudokuChromosome)bestIndividual).GetSudokus()[0];
                //nbErrors = solution.NbErrors(s);
                if (nbErrors == 0) 
                {
                    break;
                }
                else
                {
                    populationSize *= 2;
                    Console.WriteLine($"Genetic search failed with {nbErrors} resulting errors, doubling population to {populationSize}");
                }
                

            } while (true);

            for (int i = 0; i < solution.Cellules.Count; i++)
            {
                s.Cellules[i] = solution.Cellules[i];
            }

        }


       

    }

}
