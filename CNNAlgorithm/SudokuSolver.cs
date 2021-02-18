using System;
using System.Collections.Generic;
using Sudoku.Core;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CsvHelper;
using Keras;
using Keras.Models;
using Python.Runtime;
using Numpy;

namespace CNNAlgorithm
{

    class SudokuSolver : ISudokuSolver
    {

		private const string modelPath = @"C:\Users\sosth\source\repos\ECE-2021-FIN-E-Ing4-Finance-Gr02-IA1\CNNAlgorithm\Models\sudoku.model";
		private static BaseModel model = NeuralNetHelper.LoadModel(modelPath);


		public GrilleSudoku Solve(Sudoku.Core.GrilleSudoku s)
		{
			return NeuralNetHelper.SolveSudoku(s, model);
		}

        void ISudokuSolver.Solve(GrilleSudoku s)
        {
			var suudokuResolu = this.Solve(s);
			Enumerable.Range(0, 81).ToList().ForEach(i => s.Cellules[i] = suudokuResolu.Cellules[i]);
            //throw new NotImplementedException();
        }
    }


	public class SudokuSample
	{
		public Sudoku.Core.GrilleSudoku Quiz { get; set; }

		public Sudoku.Core.GrilleSudoku Solution { get; set; }

	}

	public class NeuralNetHelper
	{

		static NeuralNetHelper()
		{
			PythonEngine.PythonHome = @"C:\Users\sosth\AppData\Local\Programs\Python\Python37";
			Setup.UseTfKeras();
		}

		public static BaseModel LoadModel(string strpath)
		{
			return BaseModel.LoadModel(strpath);
		}

		public static NDarray GetFeatures(Sudoku.Core.GrilleSudoku objSudoku)
		{
			return Normalize(np.array(objSudoku.Cellules.ToArray()).reshape(9, 9));
		}

		public static Sudoku.Core.GrilleSudoku GetSudoku(NDarray features)
		{
			return new Sudoku.Core.GrilleSudoku() { Cellules = features.flatten().astype(np.int32).GetData<int>().ToList() };
		}

		public static NDarray Normalize(NDarray features)
		{
			return (features / 9) - 0.5;
		}

		public static NDarray DeNormalize(NDarray features)
		{
			return (features + 0.5) * 9;
		}



		public static Sudoku.Core.GrilleSudoku SolveSudoku(Sudoku.Core.GrilleSudoku s, BaseModel model)
		{
			var features = GetFeatures(s);
			while (true)
			{
				var output = model.Predict(features.reshape(1, 9, 9, 1));
				output = output.squeeze();
				var prediction = np.argmax(output, axis: 1).reshape(9, 9) + 1;
				var proba = np.around(np.max(output, axis: new[] { 1 }).reshape(9, 9), 1);

				features = DeNormalize(features);
				var mask = features.@equals(0);
				if (((int)mask.sum()) == 0)
				{
					break;
				}
			
				var probNew = proba * mask;
				var threshold = 1.0;
				var ind_list = np.argwhere(probNew >= threshold);
				
				while (ind_list.len == 0)
                {
					threshold = threshold - 0.1;
					if (threshold >= 0.5)
					{
						ind_list = np.argwhere(probNew >= threshold);

						if (ind_list.len > 0)
						{
							break;
						}
					}
					else
					{
						break;
					}
				}

				Console.WriteLine(ind_list);
				for (int i = 0; i < ind_list.len; i++)

					{

					var x = ind_list[i][0];
					var  y = ind_list[i][1];
					var val = prediction[x][y];
					features[x][y] = val;
					
				}
				features = Normalize(features);


			}

			return GetSudoku(features);
		}
	}





	public class DataSetHelper
	{

		public static List<SudokuSample> ParseCSV(string path, int numSudokus)
		{
			var records = new List<SudokuSample>();
			using (var compressedStream = File.OpenRead(path))
			using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
			using (var reader = new StreamReader(decompressedStream))
			using (var csv = new CsvReader(reader))
			{
				csv.Configuration.Delimiter = ",";
				csv.Read();
				csv.ReadHeader();
				var currentNb = 0;
				while (csv.Read() && currentNb < numSudokus)
				{
					var record = new SudokuSample
					{
						Quiz = Sudoku.Core.GrilleSudoku.Parse(csv.GetField<string>("quizzes")),
						Solution = Sudoku.Core.GrilleSudoku.Parse(csv.GetField<string>("solutions"))
					};
					records.Add(record);
					currentNb++;
				}
			}
			return records;
		}


	}


}
