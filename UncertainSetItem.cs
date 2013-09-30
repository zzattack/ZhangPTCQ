using System;

namespace Zhang {

	public class UncertainItem<T> : IComparable {
		public T Value { get; set; }
		public double Probability { get; set; }

		public UncertainItem(T value, double probability) {
			Value = value;
			Probability = probability;
		}

		public override bool Equals(object obj) {
			return (obj is UncertainItem<T>) && Value.Equals((obj as UncertainItem<T>).Value);
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}

		public int CompareTo(object other) {
			return ((IComparable)Value).CompareTo(((UncertainItem<T>)other).Value);
		}

		public override string ToString() {
			return string.Format("({0}, {1})", Value, Probability);
		}
	}

}
