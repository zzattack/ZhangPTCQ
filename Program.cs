using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Zhang {
	class Program {
		static void Main(string[] args) {
			// SimpleTest();

			if (args.Length != 2) {
				Console.WriteLine("Usage: Zhang <input.dat> <tests.dat>");
				return;
			}
			string inputFile = args[0];

			var r = new Random();

			// Read each line of the file into a string array. Each element of the array is one line of the file. 
			Console.WriteLine("Reading file {0}", inputFile);
			var S = new List<UncertainSet<int>>();
			const double alpha = 0.5;
			string[] lines = File.ReadAllLines(inputFile);
			foreach (string line in lines) {
				var tuple = new UncertainSet<int>();
				string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts) {
					int item = int.Parse(part);
					if (r.NextDouble() >= alpha) // 50% chance to add it to tuple as uncertain item
						tuple.Add(new UncertainItem<int>(item, r.NextDouble()));
					else // and 50% to add it as certain item
						tuple.Add(new UncertainItem<int>(item, 1.0));
				}
				S.Add(tuple);
			}

			// prepare inverted file
			var I = new InvertedFile<int>(S);

			// queryset:  randomly choose 5000 USVA values from the dataset itself 
			var q = new UncertainSet<int>();
			while (q.Count < I.index.Count / 2000) {
				var t = S[r.Next(S.Count)];
				var x = t.ElementAt(r.Next(t.Count));
				if (!q.ContainsKey(x.Key))
					q.Add(x.Value.Value, new UncertainItem<int>(x.Key, r.NextDouble()));
			}

			double tau = 0.1;
			var br = PTCQ.BasicApproach(I, q, S, tau).ToList();
			var nr = PTCQ.NaiveQuery(q, S, tau).ToList();

			Debugger.Break();
		}

	}
}
