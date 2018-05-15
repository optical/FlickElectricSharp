using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {

	[JsonObject]
	public class TokenResponse {
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty("expires_in")]
		public long ExpiresIn { get; set; }

		[JsonProperty("id_token")]
		public string IdToken { get; set; }

		[JsonProperty("token_type")]
		public string TokenType { get; set; }
	}
}
