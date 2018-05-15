using System;
using FlickElectricSharp.DTO.Responses;

namespace FlickElectricSharp {
	internal class AuthToken {
		public AuthToken(TokenResponse response) {
			AccessToken = response.AccessToken;
			IdToken = response.IdToken;
			// Assuming expires_in is in milliseconds, typical value is 5184000
			ExpiryTime = DateTime.UtcNow.AddMilliseconds(response.ExpiresIn);
		}

		public string AccessToken { get; }
		public string IdToken { get; }
		public DateTime ExpiryTime { get; }

		public bool IsExpired => DateTime.UtcNow >= ExpiryTime;
	}
}