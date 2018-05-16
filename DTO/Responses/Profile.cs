using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class Profile {

		[JsonProperty("preferred_name")]
		public string PreferredName { get; set; }

		[JsonProperty("preferred_username")]
		public string PreferredUsername { get; set; }

		[JsonProperty("updated_at")]
		public string UpdatedAt { get; set; }

	}
}