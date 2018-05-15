using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class AuthorizedDataContext {
		[JsonProperty("kind")]
		public string Kind { get; set; }

		[JsonProperty("start_at")]
		public string StartAt { get; set; }

		[JsonProperty("end_at")]
		public string EndAt { get; set; }

		[JsonProperty("identifier")]
		public string Identifier { get; set; }

	}
}