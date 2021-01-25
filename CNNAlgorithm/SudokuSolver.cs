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

		private const string modelPath = @"..\..\..\..\SolverNeuralNet\Models\sudoku.model";
		private static BaseModel model = NeuralNetHelper.LoadModel(modelPath);


		public Sudoku.Core.GrilleSudoku Solve(Sudoku.Core.GrilleSudoku s)
		{
			return NeuralNetHelper.SolveSudoku(s, model);
		}

        void ISudokuSolver.Solve(GrilleSudoku s)
        {
            throw new NotImplementedException();
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
			PythonEngine.PythonHome = @"C:\Users\sosth\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Anaconda3 (64-bit)";
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
				var prediction = np.argmax(output, axis: 2).reshape(9, 9) + 1;
				var proba = np.around(np.max(output, axis: new[] { 2 }).reshape(9, 9), 2);

				features = DeNormalize(features);
				var mask = features.@equals(0);
				if (((int)mask.sum()) == 0)
				{
					break;
				}

				var probNew = proba * mask;
				var ind = (int)np.argmax(probNew);
				var (x, y) = ((ind / 9), ind % 9);
				var val = prediction[x][y];
				features[x][y] = val;
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
