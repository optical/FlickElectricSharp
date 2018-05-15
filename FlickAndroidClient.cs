using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FlickElectricSharp.DTO.Responses;
using Newtonsoft.Json;

namespace FlickElectricSharp {
	public class FlickAndroidClient {
		private readonly string _username;
		private readonly string _password;
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly SemaphoreSlim _authLock = new SemaphoreSlim(1);

		private AuthToken _currentAuthToken;

		public FlickAndroidClient(string username, string password) {
			_username = username;
			_password = password;

			var clientHandler = new HttpClientHandler {
				AllowAutoRedirect = false
			};

			_httpClient = new HttpClient(clientHandler) {
				BaseAddress = new Uri("https://api.flick.energy")
			};
			_httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));

			_jsonSerializerSettings = new JsonSerializerSettings {
				DateTimeZoneHandling = DateTimeZoneHandling.Local
			};
		}

		public async Task<UserInfoResponse> GetUserInfo() {
			await AuthGaurd().ConfigureAwait(false);

			var response = await _httpClient.GetAsync("identity/userinfo").ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var rawBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<UserInfoResponse>(rawBody, _jsonSerializerSettings);
		}

		public Task<PriceForecastResponse> GetPriceForecast(AuthorizedDataContext dataContext) {
			return GetPriceForecast(dataContext.Identifier);
		}

		public async Task<PriceForecastResponse> GetPriceForecast(string identifier) {
			var response = await _httpClient.GetAsync($"rating/forecast_prices?supply_node={HttpUtility.UrlEncode(identifier)}&number_of_periods_ahead=1").ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			
			var rawBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<PriceForecastResponse>(rawBody, _jsonSerializerSettings);
		}

		private async Task AuthGaurd() {
			try {
				await _authLock.WaitAsync().ConfigureAwait(false);
				if (_currentAuthToken == null || _currentAuthToken.IsExpired) {
					await Authenticate().ConfigureAwait(false);
				}
			} finally {
				_authLock.Release();
			}
		}

		private async Task Authenticate() {
			_httpClient.DefaultRequestHeaders.Authorization = null;

			const string clientId = "igex5x0k6z2xtki8xdyykta22adg0mr";
			const string codeChallenge = "5kuC6U1wdMCWd30oV1HUYsffKtC-3BFNJR5uFNPtSzw";
			const string codeChallengeMethod = "S256";
			const string state = "zE8Si66zYW7nJJtGAaok1w";
			const string responseType = "code";
			const string redirectUri = "flickapp://nz.co.flickelectric.androidapp";
			
			// Much of this content is sniffed and replayed without understanding its meaning
			var content = new FormUrlEncodedContent(new Dictionary<string, string> {
				["authenticity_token"] = "bKonPuDWT1zENHR66AewX2al+Tv8gzH5RUGBqXFM6x2SqrrFgTPw+ftWccSwhgDhCfGQtyWpn6y3mU1Vc3UUKg==", // Possibly some form of CSRF token?
				["button"] = "",
				["client_id"] = clientId,
				["code_challenge"] = codeChallenge,
				["code_challenge_method"] = codeChallengeMethod,
				["redirect_uri"] = redirectUri,
				["response_type"] = responseType,
				["state"] = state,
				["user[email]"] = _username,
				["user[guest]"] = "false",
				["user[password]"] = _password,
				["user[remember_me]"] = "1", // The app sends this twice, with a value of 0 and again with value of 1. Possibly a bug in the app, lets just use 1?
				["utf8"] = "✓"
			});

			var rawLoginUrl = $"identity/users/sign_in?client_id={clientId}&code_challenge={codeChallenge}&code_challenge_method={codeChallengeMethod}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code&state={state}";

			var result = await _httpClient.PostAsync(rawLoginUrl, content).ConfigureAwait(false);
			if (result.StatusCode != HttpStatusCode.Redirect) {
				throw new Exception($"sign_in request returned a status code of {result.StatusCode}, was expecting {HttpStatusCode.Redirect}");
			}

			var locationQueryString = HttpUtility.ParseQueryString(result.Headers.Location.Query);
			var code = locationQueryString["code"];

			var oauthTokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string> {
				["client_id"] = clientId,
				["code"] = code,
				["code_verifier"] = "m0XAr3bM42VzUx9ayQXiGgkzliElmhQFdpM-RmpMv-Gr2BsdTYDkAf46VLoH72lI6Z_ZGn4DV0xOYFqQ4_cecw",
				["grant_type"] = "authorization_code",
				["redirect_uri"] = redirectUri
			});

			var tokenRequest = await _httpClient.PostAsync("identity/oauth/token", oauthTokenRequestBody).ConfigureAwait(false);
			// Expect a 200 OK here, so can verify the code.
			tokenRequest.EnsureSuccessStatusCode();

			var rawTokenResponse = await tokenRequest.Content.ReadAsStringAsync().ConfigureAwait(false);
			var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(rawTokenResponse, _jsonSerializerSettings);

			_currentAuthToken = new AuthToken(tokenResponse);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentAuthToken.IdToken);
		}
	}
}
