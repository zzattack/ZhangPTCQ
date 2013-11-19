using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Zhang {
	public static class PTCQ {

		public static IEnumerable<UncertainSet<T>> NaiveQuery<T>(UncertainSet<T> q, IEnumerable<UncertainSet<T>> S, double tau)
		where T : IComparable {
			return S.Where(s => UncertainSet<T>.Theorem1(q, s) >= tau);
		}

		public static IEnumerable<UncertainSet<T>> BasicTheorem1<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau)
		where T : IComparable {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau)
				return new HashSet<UncertainSet<T>>(S);

			// 2. C_q <- \Cup_{x \in q}I(x) -- i.e. candidates with at least one entry in inverted index
			var candidates = new HashSet<UncertainSet<T>>();
			foreach (var candidate in q.Values.SelectMany(x => invFile.GetCandidates(x)))
				candidates.Add(candidate);

			// Note that the paper mentions a more 'manual' way of applying Th.1 by
			// keeping track of partial probabilities. This offers no performance increase
			// over simply creating a union of the candidates and applying Th.1 over that.

			// 3. filter candidates using Th. 1
			return candidates.Where(c => UncertainSet<T>.Theorem1(q, c) >= tau);
		}


		public static IEnumerable<UncertainSet<T>> BasicHash<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau)
		where T : IComparable {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau)
				return new HashSet<UncertainSet<T>>(S);

			// collect all candidates
			var candidates = new HashSet<UncertainSet<T>>();
			foreach (var qi in q.Values) {
				// disregard the tuples not in the inverted file
				foreach (var c in invFile.GetCandidates(qi))
					candidates.Add(c);
			}

			// update all partial probabilities
			var partialProbabilities = new Dictionary<UncertainSet<T>, double>();
			foreach (var qi in q.Values) {
				foreach (var t in candidates)
					UpdatePartialProbability(partialProbabilities, qi, t, tau);
			}

			// Note that the paper mentions a more 'manual' way of applying Th.1 by
			// keeping track of partial probabilities. This offers no performance increase
			// over simply creating a union of the candidates and applying Th.1 over that.

			return candidates.Where(c => partialProbabilities[c] >= tau);
		}

		public enum RefineStrategy {
			ByData,
			ByIndex,
			Hybrid
		};

		public static IEnumerable<UncertainSet<T>> EnhancedApproach<T>(InvertedFile<T> invFile, UncertainSet<T> q, List<UncertainSet<T>> S, double tau, RefineStrategy strat)
		where T : IComparable {
			// 1. if q(\emptySet) >= \tau then return S
			if (q.WorldProbability(new HashSet<T>()) >= tau)
				return new HashSet<UncertainSet<T>>(S);

			// determine pruning and non-pruning items
			var u = new Dictionary<UncertainItem<T>, double>(); // pruning items
			var nonPruningItems = new List<UncertainItem<T>>();
			foreach (var x in q.Values) {
				double c = 1 - (1.0 - tau) / x.Probability;
				if (c > 0) // x is a cutoff item
					u[x] = c;
				else
					nonPruningItems.Add(x);
			}

			// 2. C_q <- \Cap_{x \in PI}{ (s,s(x)) in I(x) | s(x) >= c(x) } -- i.e. candidates
			var partialProbabilities = new Dictionary<UncertainSet<T>, double>();
			var candidates = S.ToList();
			foreach (var kvp in u) {
				var partialCandidates = invFile.GetCandidates(kvp.Key, kvp.Value);
				var pi = kvp.Key;

				// candidates = candidates.Intersect(partialCandidates).ToList();
				foreach (var t in partialCandidates) {
					UpdatePartialProbability(partialProbabilities, pi, t, tau);
				}
				// disregard the tuples not in the inverted file
				candidates = candidates.Intersect(partialCandidates).ToList();
			}

			// note that this requires that we also look up each x \in q - PI(q) to further refine C_q
			switch (strat) {
				case RefineStrategy.ByData:
					RefineByData(S, tau, nonPruningItems, partialProbabilities);
					break;

				case RefineStrategy.ByIndex:
					RefineByIndex(invFile, S, nonPruningItems, partialProbabilities);
					break;

				case RefineStrategy.Hybrid:
					// determine cost
					const double beta = 10; // avg # IO's required to access tuple in S
					const double gamma = 10; // avg random IO cost
					const double delta = 1; // avg sequential IO cost
					double cost1 = q.Count * beta * gamma;
					double cost2 = delta * nonPruningItems.Sum(s => invFile.GetCandidates(s).Count);

					if (cost1 < cost2) RefineByData(S, tau, nonPruningItems, partialProbabilities);
					else RefineByIndex(invFile, S, nonPruningItems, partialProbabilities);
					break;
			}

			return candidates.Where(c => partialProbabilities[c] >= tau);
		}

		private static void RefineByIndex<T>(InvertedFile<T> invFile, List<UncertainSet<T>> S, List<UncertainItem<T>> nonPruningItems, Dictionary<UncertainSet<T>, double> partialProbabilities) where T : IComparable {

			foreach (var npi in nonPruningItems) {
				var IS = invFile.GetCandidates(npi);

				foreach (var t in IS) {
					double v;
					if (!partialProbabilities.TryGetValue(t, out v))
						v = 1.0;
					double tp = t[npi.Value].Probability;
					v *= npi.Probability * tp + 1 - npi.Probability;
					partialProbabilities[t] = v;
				}

				foreach (var t in S.Except(IS)) {
					double v;
					if (!partialProbabilities.TryGetValue(t, out v))
						v = 1.0;
					double tp = 0;
					v *= npi.Probability * tp + 1 - npi.Probability;
					partialProbabilities[t] = v;
				}

			}
		}

		private static void RefineByData<T>(List<UncertainSet<T>> S, double tau, List<UncertainItem<T>> nonPruningItems, Dictionary<UncertainSet<T>, double> partialProbabilities) where T : IComparable {
			foreach (var t in S)
				foreach (var qi in nonPruningItems)
					UpdatePartialProbability(partialProbabilities, qi, t, tau);
		}

		private static void UpdatePartialProbability<T>(Dictionary<UncertainSet<T>, double> partialProbabilities,
			UncertainItem<T> qi, UncertainSet<T> t, double tau) where T : IComparable {

			double v;
			if (!partialProbabilities.TryGetValue(t, out v))
				v = 1.0;
			double tp = t.ContainsKey(qi.Value) ? t[qi.Value].Probability : 0;
			v *= qi.Probability * tp + 1 - qi.Probability;

			partialProbabilities[t] = v;
		}

	}
}
