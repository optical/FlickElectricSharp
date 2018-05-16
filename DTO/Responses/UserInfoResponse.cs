using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class UserInfoResponse {
		[JsonProperty("sub")]
		public string Sub { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("email_verified")]
		public bool EmailVerified { get; set; }

		[JsonProperty("profile")]
		public Profile Profile { get; set; }

		[JsonProperty("allowed_activities")]
		public string[] AllowedActivities { get; set; }

		[JsonProperty("authorized_data_contexts")]
		public AuthorizedDataContexts AuthorizedDataContexts { get; set; }

	}
}
