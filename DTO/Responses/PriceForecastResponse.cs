using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class PriceForecastResponse {
		[JsonProperty("kind")]
		public string Kind { get; set; }

		[JsonProperty("prices")]
		public PriceDescription[] Prices { get; set; }
	}
}
