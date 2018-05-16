using Newtonsoft.Json;

namespace FlickElectricSharp.DTO.Responses {
	[JsonObject]
	public class AuthorizedDataContexts {

		[JsonProperty("billing_entities")]
		public AuthorizedDataContext[] BillingEntities { get; set; }

		[JsonProperty("supply_nodes")]
		public AuthorizedDataContext[] SupplyNodes { get; set; }

	}
}