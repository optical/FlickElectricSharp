using System;

namespace FlickElectricSharp {
	public class PowerAndPriceInterval {
		public PowerAndPriceInterval(DateTime start, DateTime end, double price, double units) {
			Start = start;
			End = end;
			Price = price;
			Units = units;
		}

		public DateTime Start { get; }
		public DateTime End { get; }
		public double Price { get; }
		public double Units { get; }
	}
}
