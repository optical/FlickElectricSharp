using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using HtmlAgilityPack;

namespace FlickElectricSharp {
	public class FlickWebClient {
		private readonly string _username;
		private readonly string _password;
		private readonly HttpClient _httpClient;
		private readonly SemaphoreSlim _authLock = new SemaphoreSlim(1);
		private bool _isAuthed;

		public FlickWebClient(string username, string password) {
			_username = username;
			_password = password;
			_httpClient = new HttpClient();
		}

		// Does not handle expiry properly
		private async Task AuthGuard() {
			try {
				await _authLock.WaitAsync().ConfigureAwait(false);
				if (!_isAuthed) {
					await Login().ConfigureAwait(false);
				}
			} finally {
				_authLock.Release();
			}
		}

		private async Task Login() {
			var response = await _httpClient.GetAsync("https://id.flickelectric.co.nz/identity/users/sign_in").ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			var rawPage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(rawPage);
			var crsfMetaTag = doc.DocumentNode.Descendants("meta")
				.Single(metaTag => metaTag.Attributes.Any(attr => attr.Value == "csrf-token"));

			var csrfToken = crsfMetaTag.Attributes.Single(attr => attr.Name == "content").Value;

			var content = new FormUrlEncodedContent(new Dictionary<string, string> {
				["authenticity_token"] = csrfToken,
				["button"] = "",
				["user[email]"] = _username,
				["user[guest]"] = "",
				["user[password]"] = _password,
				["user[remember_me]"] = "1",
				["utf8"] = "✓"
			});

			var loginResponse = await _httpClient.PostAsync("https://id.flickelectric.co.nz/identity/users/sign_in", content).ConfigureAwait(false);
			loginResponse.EnsureSuccessStatusCode();
			_isAuthed = true;
		}

		public async Task<IList<PowerUsageBucket>> GetPowerUsage(DateTime start, DateTime end) {
			await AuthGuard().ConfigureAwait(false);

			var fromDate = start.ToString("yyyy/MM/dd");
			var toDate = end.ToString("yyyy/MM/dd");

			var data = await _httpClient.GetAsync($"https://myflick.flickelectric.co.nz/dashboard/download_usage_data.csv?from_date={fromDate}&to_date={toDate}").ConfigureAwait(false);
			var powerUsageBuckets = new List<PowerUsageBucket>();

			using (var csvReader = new CsvReader(new StreamReader(await data.Content.ReadAsStreamAsync().ConfigureAwait(false)))) {
				await csvReader.ReadAsync().ConfigureAwait(false);
				csvReader.ReadHeader();

				while (await csvReader.ReadAsync().ConfigureAwait(false)) {
					var icpNumber = csvReader.GetField<string>("icp_number");
					var meterSerialNumber = csvReader.GetField<string>("meter_serial_number");
					var channelNumber = csvReader.GetField<string>("channel_number");
					var startedAt = csvReader.GetField<DateTime>("started_at");
					var endedAt = csvReader.GetField<DateTime>("ended_at");
					var value = csvReader.GetField<double>("value");
					var unitCode = csvReader.GetField<string>("unit_code");
					var status = csvReader.GetField<string>("status");

					var powerUsageBucket = new PowerUsageBucket(icpNumber, meterSerialNumber, channelNumber, startedAt, endedAt, value, unitCode, status);
					powerUsageBuckets.Add(powerUsageBucket);
				}
			}

			return powerUsageBuckets;
		}
	}
}

