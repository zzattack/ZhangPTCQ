using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Zhang {

	[Serializable]
	public class InvertedFile<T> where T : IComparable {

		public SortedDictionary<T, List<UncertainSet<T>>> index = new SortedDictionary<T, List<UncertainSet<T>>>();

		public InvertedFile(IEnumerable<UncertainSet<T>> S) {
			// build inverted file from all items in all tupes in S
			foreach (var tuple in S) {
				foreach (var item in tuple.Values) {
					List<UncertainSet<T>> list;
					if (!index.TryGetValue(item.Value, out list))
						list = index[item.Value] = new List<UncertainSet<T>>();
					list.Add(tuple);
				}
			}

			// sort each list by descending probability of tuple containing the key
			foreach (var key in index.Keys) {
				var list = index[key];
				list.Sort((x, y) => -x[key].Probability.CompareTo(y[key].Probability));
			}
		}

		// save to disk
		public void Serialize(Stream s) {
			var xs = new XmlSerializer(typeof(InvertedFile<T>));
			xs.Serialize(s, this);
		}

		// load from disk
		public static InvertedFile<T> Deserialize(Stream s) {
			var xs = new XmlSerializer(typeof(InvertedFile<T>));
			var ret = (InvertedFile<T>)xs.Deserialize(s);
			return ret;
		}

		public List<UncertainSet<T>> GetCandidates(UncertainItem<T> x) {
			return index[x.Value];
		}

		public List<UncertainSet<T>> GetCandidates(UncertainItem<T> x, double cutoff) {
			var list = index[x.Value];
			int cutOffIdx = list.FindIndex(0, list.Count, set => cutoff > set[x.Value].Probability);

			if (cutOffIdx < 0) return new List<UncertainSet<T>>();
			return list.GetRange(0, cutOffIdx);
		}

		public List<UncertainSet<T>> GetNonCandidates(UncertainItem<T> x, double cutoff) {
			var list = index[x.Value];
			int cutOffIdx = list.FindIndex(0, list.Count, set => cutoff > set[x.Value].Probability);
			return list.GetRange(cutOffIdx, list.Count - cutOffIdx);
		}


		internal SortedDictionary<T, List<UncertainSet<T>>> GetIndex() {
			return index;
		}
	}
}
