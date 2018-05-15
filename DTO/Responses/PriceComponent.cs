using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class PriceComponent {

		[JsonProperty("kind")]
		public string Kind { get; set; }

		[JsonProperty("charge_method")]
		public string ChargeMethod { get; set; }

		[JsonProperty("charge_setter")]
		public string ChargeSetter { get; set; }

		[JsonProperty("value")]
		public double Value { get; set; }

		[JsonProperty("unit_code")]
		public string UnitCode { get; set; }

		[JsonProperty("per")]
		public string PerUnit { get; set; }
	}
}