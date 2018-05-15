using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class PriceValue {
		[JsonProperty("value")]
		public double Value { get; set; }

		[JsonProperty("unit_code")]
		public string UnitCode { get; set; }

		[JsonProperty("per")]
		public string PerUnit { get; set; }
	}
}