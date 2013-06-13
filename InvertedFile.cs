using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhang {
	public class InvertedFile<T> {
		// TODO: change to use SQLite DB for persistent storage
		Dictionary<T, List<UncertainSet<T>>> index = new Dictionary<T, List<UncertainSet<T>>>();

		public InvertedFile(List<UncertainSet<T>> S) {
			for (int i = 0; i < S.Count; i++) {
				var s = S[i];
				foreach (var x in s) {
					List<UncertainSet<T>> list;
					if (!index.TryGetValue(x.Value, out list))
						list = index[x.Value] = new List<UncertainSet<T>>();
					list.Add(s);
				}
			}
			// this method of constructing the dictionary guarantees that all lists are sorted already
		}

		internal List<UncertainSet<T>> GetCandidates(UncertainItem<T> x) {
			return index[x.Value];
		}

	}
}
