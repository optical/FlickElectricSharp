using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

		public async Task<IList<PowerAndPriceInterval>> FetchDetailedUsageForDay(DateTime queryDateTime) {
			await AuthGuard().ConfigureAwait(false);

			var url = $"https://myflick.flickelectric.co.nz/dashboard/day/{queryDateTime:yyyy-MM-dd}";
			var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			var rawPage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var doc = new HtmlDocument();
			doc.LoadHtml(rawPage);

			// Crude error check, since it returns a 200 OK even when things go bad
			if (rawPage.Contains("please select another day")) {
				throw new FlickElectricException("There is an issue with the page being returned. Either data is not available for the day, or something else went wrong", response);
			}

			var dateNavigator = doc.GetElementbyId("date-navigation-calendar").InnerHtml.Trim();
			if (DateTime.Parse(dateNavigator).Date != queryDateTime.Date) {
				throw new FlickElectricException($"Got data back for an incorrect date. Got {dateNavigator}, but was expecting for {queryDateTime.Date}", response);
			}

			var rows = doc.GetElementbyId("day-table").Descendants("tbody").Single().Descendants("tr").ToList();

			var unitsRegex = new Regex(@"^(?<units>\d+.\d+) units$");
			var priceRegex = new Regex(@"^(?<price>\d+.\d+)¢$");
			var intervalRegex = new Regex(@"(?<start>\d+:\d+) to (?<end>\d+:\d+)");

			// should be 48 intervals in a day
			var result = new List<PowerAndPriceInterval>(48);
			foreach (var rowNode in rows) {
				var price = double.Parse(
					rowNode.Descendants("td")
					.Select(node => priceRegex.Match(node.InnerHtml))
					.Single(match => match.Success)
					.Groups["price"].Value);

				var units = double.Parse(
					rowNode.Descendants("td")
					.Select(node => unitsRegex.Match(node.InnerHtml))
					.Single(match => match.Success)
					.Groups["units"].Value);


				var intervalMatch = intervalRegex.Match(rowNode.Descendants("th").Single().InnerHtml);
				if (!intervalMatch.Success) {
					throw new FlickElectricException("Unable to find interval in resulting page. Flick may have changed their page layout", response);
				}

				var startTime = TimeSpan.Parse(intervalMatch.Groups["start"].Value);
				var endTime = TimeSpan.Parse(intervalMatch.Groups["end"].Value);

				result.Add(new PowerAndPriceInterval(queryDateTime.Date + startTime, queryDateTime.Date + endTime, price, units));
			}

			return result;
		}
	}
}

