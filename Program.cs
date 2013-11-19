using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Zhang {
	class Program {
		static Random rnd = new Random();
		static Stopwatch sw = Stopwatch.StartNew();

		static void Main(string[] args) {
			// SimpleTest();

			if (args.Length != 2) {
				Console.WriteLine("Usage: Zhang <input.dat> <tests.dat>");
				return;
			}
			string inputFile = args[0];

			// Read each line of the file into a string array. Each element of the array is one line of the file. 
			Console.WriteLine("Reading file {0}", inputFile);
			//var S = LoadIBMDataset(inputFile); // generates random probabilities
			//SaveQueryset(S, "IBM_1.dat");
			var S = LoadDataset("IBM_1.dat");

			// prepare inverted file
			var I = new InvertedFile<int>(S);

			// queryset:  randomly choose 5000 USVA values from the dataset itself 
			// var q = GenerateRandomQueryset(S, I);
			var q = LoadDataset("IBM_q1.dat").First();

			double tau = 0.25;
			PrintTime("init");

			var bt = PTCQ.BasicTheorem1(I, q, S, tau).ToList();
			PrintTime("basic th1");

			var bh = PTCQ.BasicHash(I, q, S, tau).ToList();
			PrintTime("basichash");

			var nq = PTCQ.NaiveQuery(q, S, tau).ToList();
			PrintTime("naivequery");

			var eai = PTCQ.EnhancedApproach(I, q, S, tau, PTCQ.RefineStrategy.ByIndex).ToList();
			PrintTime("enhanced byindex");

			var ead = PTCQ.EnhancedApproach(I, q, S, tau, PTCQ.RefineStrategy.ByData).ToList();
			PrintTime("enhanced bydata");

			var eah = PTCQ.EnhancedApproach(I, q, S, tau, PTCQ.RefineStrategy.Hybrid).ToList();
			PrintTime("enhanced hybrid");

			Debugger.Break();
		}

		private static void PrintTime(string pre) {
			Console.WriteLine("{0:d4}:\t{1}", pre, sw.ElapsedMilliseconds);
			sw.Reset();
			sw.Start();
		}

		static List<UncertainSet<int>> LoadIBMDataset(string inputFile) {
			var S = new List<UncertainSet<int>>();
			const double alpha = 0.5;
			string[] lines = File.ReadAllLines(inputFile);
			foreach (string line in lines) {
				var tuple = new UncertainSet<int>();
				string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts) {
					int item = int.Parse(part);
					if (rnd.NextDouble() >= alpha) // 50% chance to add it to tuple as uncertain item
						tuple.Add(new UncertainItem<int>(item, rnd.NextDouble()));
					else // and 50% to add it as certain item
						tuple.Add(new UncertainItem<int>(item, 1.0));
				}
				S.Add(tuple);
			}
			return S;
		}

		static UncertainSet<T> GenerateRandomQueryset<T>(List<UncertainSet<T>> S, InvertedFile<T> I)
		where T : IComparable {
			var q = new UncertainSet<T>();
			while (q.Count < I.index.Count / 2500) {
				var t = S[rnd.Next(S.Count)];
				var x = t.ElementAt(rnd.Next(t.Count));
				if (!q.ContainsKey(x.Key))
					q.Add(x.Value.Value, new UncertainItem<T>(x.Key, 0.2 + rnd.NextDouble() * 0.8));
			}
			return q;
		}

		static List<UncertainSet<int>> LoadDataset(string path) {
			var ret = new List<UncertainSet<int>>();
			string[] lines = File.ReadAllLines(path);
			foreach (var line in lines) {
				var tuple = new UncertainSet<int>();
				foreach (var item in line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
					var ps = item.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					int key = int.Parse(ps[0]);
					double probability = double.Parse(ps[1]);
					tuple.Add(new UncertainItem<int>(key, probability));
				}
				ret.Add(tuple);
			}
			return ret;
		}

		public static void SaveQueryset<T>(UncertainSet<T> q, string path) where T : IComparable {
			SaveQueryset(new[] { q }, path);
		}
		public static void SaveQueryset<T>(IEnumerable<UncertainSet<T>> set, string path) where T : IComparable {
			var sb = new StringBuilder();
			foreach (var tuple in set) {
				foreach (var item in tuple) {
					sb.AppendFormat("{0}:{1} ", item.Key, item.Value.Probability);
				}
				sb.AppendLine();
			}
			File.WriteAllText(path, sb.ToString());
		}

	}
}
