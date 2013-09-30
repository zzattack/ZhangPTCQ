using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhang {
	public static class PTCQ {

		public static IEnumerable<UncertainSet<T>> NaiveQuery<T>(UncertainSet<T> q, IEnumerable<UncertainSet<T>> S, double tau) {
			return S.Where(s => UncertainSet<T>.Theorem1(q, s) >= tau);
		}

		public static IEnumerable<UncertainSet<T>> BasicApproach<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau) {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau)
				return new HashSet<UncertainSet<T>>(S);

			// 2. C_q <- \Cup_{x \in q}I(x) -- i.e. candidates
			var candidates = new HashSet<UncertainSet<T>>();
			foreach (var x in q.Values)
				foreach (var candidate in invFile.GetCandidates(x))
					candidates.Add(candidate);

			// 3. filter candidates using Th. 1
			return candidates.Where(c => UncertainSet<T>.Theorem1(q, c) >= tau);
		}

		public enum RefineStrategy {
			ByData,
			ByIndex,
			Hybrid
		};

		public static IEnumerable<UncertainSet<T>> EnhancedApproach<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau, RefineStrategy strat) {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau)
				return new HashSet<UncertainSet<T>>(S);

			// 2. C_q <- \Cap_{x \in PI}{ (s,s(x)) in I(x) | s(x) >= c(x) } -- i.e. candidates
			var pruningItems = new Dictionary<UncertainItem<T>, double>();
			var nonPruningItems = new List<UncertainItem<T>>();
			foreach (var x in q.Values) {
				double c = 1 - (1.0 - tau) / x.Probability;
				if (c > 0) // x is a cutoff item
					pruningItems[x] = c;
				else
					nonPruningItems.Add(x);
			}
			IEnumerable<UncertainSet<T>> candidates = S.ToList();
			foreach (var x in pruningItems)
				candidates = candidates.Intersect(invFile.GetCandidates(x.Key, x.Value));

			foreach (var x in nonPruningItems) {
				switch (strat) {
					case RefineStrategy.ByData:
						// 3. refine candidates using Th. 1
						return candidates.Where(c => UncertainSet<T>.Theorem1(q, c) >= tau);

					case RefineStrategy.ByIndex:
						return candidates.Where(c => UncertainSet<T>.Theorem1(q, c) >= tau);

					case RefineStrategy.Hybrid:
						break;
				}
			}

			return null;
		}

	}
}
