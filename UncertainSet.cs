using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Zhang {
	public class UncertainSet<T> : SortedList<T, UncertainItem<T>> where T : IComparable {
		private static int _idCounter = 0;
		public int Id { get; private set; }

		public UncertainSet() {
			Id = _idCounter++;
		}

		public HashSet<T> AsSet() {
			return new HashSet<T>(this.Values.Select(v => v.Value));
		}
		public List<T> AsList() {
			return new List<T>(this.Values.Select(v => v.Value));
		}

		public double WorldProbability(IEnumerable<T> world) {
			// "naive" evaluation of world probability;
			return this.Values.Aggregate<UncertainItem<T>, double>(1, (current, i) => current * (world.Contains(i.Value) ? i.Probability : 1 - i.Probability));
		}

		public static double Theorem1(UncertainSet<T> r, UncertainSet<T> s) {
			double res = 1.0;
			foreach (var rx in r.Values) {
				UncertainItem<T> sx = null;
				s.TryGetValue(rx.Value, out sx);
				res *= rx.Probability * (sx != null ? sx.Probability : 0) + 1 - rx.Probability;
			}
			return res;
		}

		public override string ToString() {
			return Id.ToString(CultureInfo.InvariantCulture);
		}

		public void Add(UncertainItem<T> uncertainItem) {
			Add(uncertainItem.Value, uncertainItem);
		}
	}
}
