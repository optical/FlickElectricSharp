using System;
using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class PriceDescription {
		[JsonProperty("kind")]
		public string Kind { get; set; }

		[JsonProperty("start_at")]
		public DateTime StartsAt { get; set; }

		[JsonProperty("end_at")]
		public DateTime EndsAt { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("channel_ref")]
		public string ChannelReference { get; set; }

		[JsonProperty("display_price")]
		public bool DisplayPrice { get; set; }

		[JsonProperty("charge_methods")]
		public string[] ChargeMethods { get; set; }

		[JsonProperty("price")]
		public PriceValue Price{ get; set; }

		[JsonProperty("components")]
		public PriceComponent[] Components { get; set; }
	}
}