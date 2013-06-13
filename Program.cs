using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Zhang {
	class Program {
		static void Main(string[] args) {

			var r1 = new UncertainSet<string>();
			r1.Add(new UncertainItem<string>("automotive", 0.5));
			r1.Add(new UncertainItem<string>("commercial", 0.75));
			r1.Add(new UncertainItem<string>("hobby", 0.2));
			var r2 = new UncertainSet<string>();
			r2.Add(new UncertainItem<string>("automotive", 0.5));
			r2.Add(new UncertainItem<string>("hobby", 0.2));
			var r3 = new UncertainSet<string>();
			r3.Add(new UncertainItem<string>("commercial", 0.75));

			var S = new List<UncertainSet<string>>();
			S.Add(r1); S.Add(r2); S.Add(r3);

			var q = new UncertainSet<string>();
			q.Add(new UncertainItem<string>("commercial", 0.6));
			q.Add(new UncertainItem<string>("hobby", 0.2));

			var result1 = PTCQ<string>.NaiveQuery(q, S, 0.5);

			var invFile = new InvertedFile<string>(S);
			var result2 = PTCQ<string>.BasicQuery(invFile, q, S, 0.5);

			Debug.Assert(result1.Intersect(result2).Count() == result1.Count());
		}
	}
}
