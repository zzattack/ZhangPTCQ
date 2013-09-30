using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhang {
	public class InvertedFile<T> {
		// TODO: change to use SQLite DB for persistent storage
		internal SortedDictionary<T, List<UncertainSet<T>>> index = new SortedDictionary<T, List<UncertainSet<T>>>();

		public InvertedFile(List<UncertainSet<T>> S) {
			for (int i = 0; i < S.Count; i++) {
				var s = S[i];
				foreach (var x in s.Values) {
					List<UncertainSet<T>> list;
					if (!index.TryGetValue(x.Value, out list))
						list = index[x.Value] = new List<UncertainSet<T>>();
					list.Add(s);
				}
			}

			// sort each list by descending probability of tuple containing the key
			foreach (var key in index.Keys) {
				var list = index[key];
				list.Sort((x, y) => -x[key].Probability.CompareTo(y[key].Probability));
			}
		}

		internal List<UncertainSet<T>> GetCandidates(UncertainItem<T> x) {
			return index[x.Value];
		}

		internal List<UncertainSet<T>> GetCandidates(UncertainItem<T> x, double cutoff) {
			var list = index[x.Value];
			int cutOffIdx = list.FindIndex(0, list.Count, set => cutoff > set[x.Value].Probability);
			return list.GetRange(0, cutOffIdx);
		}

	}
}
