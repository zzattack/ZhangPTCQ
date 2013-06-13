using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhang {
	public class UncertainSet<T> : List<UncertainItem<T>> {

		public HashSet<T> AsSet() {
			return new HashSet<T>(this.Select(i => i.Value));
		}
		public List<T> AsList() {
			return new List<T>(this.Select(i => i.Value));
		}

		public double WorldProbability(IEnumerable<T> w) {
			double r = 1;
			foreach (var i in this)
				r *= w.Contains(i.Value) ? i.Probability : 1 - i.Probability;
			return r;
		}

		public double ContainmentProbability(UncertainSet<T> s) {
			return ContainmentProbabilityRecurse(
				this, new List<T>(), this.ToList(),
				s, new List<T>(), s.ToList());
		}


		// EXTREMELY slow implementation of the most basic algorithm
		// -- this serves only as a way of verifying the implemented solution's correctness
		static double ContainmentProbabilityRecurse(
			UncertainSet<T> r, List<T> world1, List<UncertainItem<T>> remainingChoices1,
			UncertainSet<T> s, List<T> world2, List<UncertainItem<T>> remainingChoices2) {

			double probabilitySum = 0.0;
			
			if (remainingChoices1.Count == 0) {
				var excSet = world1.Except(world2);
				if (!excSet.Any())
					probabilitySum = r.WorldProbability(world1) * s.WorldProbability(world2);
			}
			else {
				var choice = remainingChoices1[0];
				remainingChoices1.RemoveAt(0);

				world1.Add(choice.Value);
				probabilitySum += ContainmentProbabilityRecurseInner(r, world1, s, world2, new List<UncertainItem<T>>(remainingChoices2));

				// undo choice
				world1.Remove(choice.Value);
				probabilitySum += ContainmentProbabilityRecurseInner(r, world1, s, world2, new List<UncertainItem<T>>(remainingChoices2));
			}

			return probabilitySum;
		}

		// this inner loop backtracks over all possible worlds of s
		static double ContainmentProbabilityRecurseInner(UncertainSet<T> r, List<T> world1, UncertainSet<T> s, List<T> world2, List<UncertainItem<T>> remainingChoices2) {
			double probabilitySum = 0.0;

			if (remainingChoices2.Count == 0) {
				var excSet = world1.Except(world2);
				if (!excSet.Any())
					probabilitySum = r.WorldProbability(world1) * s.WorldProbability(world2);
			}
			else {
				var choice = remainingChoices2[0];
				remainingChoices2.RemoveAt(0);

				world2.Add(choice.Value);
				probabilitySum += ContainmentProbabilityRecurseInner(r, world1, s, world2, new List<UncertainItem<T>>(remainingChoices2));

				world2.Remove(choice.Value);
				probabilitySum += ContainmentProbabilityRecurseInner(r, world1, s, world2, new List<UncertainItem<T>>(remainingChoices2));
			}

			return probabilitySum;
		}



	}
}
