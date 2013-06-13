using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhang {
	public static class PTCQ<T> {


		public static HashSet<UncertainSet<T>> NaiveQuery<T>(UncertainSet<T> q, IEnumerable<UncertainSet<T>> S, double tau) {
			var ret = new HashSet<UncertainSet<T>>();
			foreach (UncertainSet<T> s in S) {
				if (q.ContainmentProbability(s) >= tau)
					ret.Add(s);
			}
			return ret;
		}


		public static IEnumerable<UncertainSet<T>> BasicQuery<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau) {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau) return new HashSet<UncertainSet<T>>(S);

			// 2. C_q <- \Cup_{x \in q}I(x) -- i.e. candidates
			var C_q = new List<UncertainSet<T>>();
			foreach (var t in q) {
				foreach (var candidate in invFile.GetCandidates(t))
					if (!C_q.Contains(candidate))
						C_q.Add(candidate);
			}

			// 3.  -- i.e. filter candidates using Th. 1
			var result = new List<UncertainSet<T>>();
			foreach (var candidate in C_q) {
				bool prune = false;
				foreach (var x in q) {
					var itemIdx = candidate.BinarySearch(0, candidate.Count, x, null);
					double s_x = itemIdx >= 0 ? candidate[itemIdx].Probability : 0;
					if (s_x < 1.0 - ((1.0 - tau) / x.Probability)) {
						prune = true; break;
					}
				}
				if (!prune)
					result.Add(candidate);

			}
			return result;
		}


		public static HashSet<UncertainSet<T>> EnhanhedQuery<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau) {
			// TODO
			return new HashSet<UncertainSet<T>>();
		}

	}
}
