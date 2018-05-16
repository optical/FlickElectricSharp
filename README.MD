# FlickElectricSharp [![Build Status](https://travis-ci.org/optical/FlickElectricSharp.svg?branch=master)](https://travis-ci.org/optical/FlickElectricSharp) [![Build status](https://ci.appveyor.com/api/projects/status/9x0eoknwn9tywgua?svg=true)](https://ci.appveyor.com/project/optical/flickelectricsharp)

A simple, unofficial .NET client for accessing current [FlickElectric]("https://flickelectric.co.nz) prices, along with historic data

## Installation
Nuget package, coming soon

## Usage
There are 2 main classes for accessing the Flick API's. One corresponds to their web api for exporting historic usage data, while the other uses their Android APP api to poll for current and predicted future pricing.

See the Flick2Influx project for an example application which queries the API and stores the results in InfluxDB.

### FlickAndroidClient
Provides access to the current flick prices

```
var client = new FlickAndroidClient("username", "password");
// Get our user details
var userInfo = await client.GetUserInfo();

// Get current + forecast prices
var forecastPrices = await client.GetPriceForecast(userInfo.AuthorizedDataContexts.SupplyNodes[0]);

// Of all the forecasts, choose the current one.
var currentPredictedPrice = forecastPrices.Prices.MinBy(price => price.StartsAt);

// Details  about how much the spot price is, along with retailer fees, line fees, etc that make up the total price
foreach (var priceComponent in currentPredictedPrice.Components) {
	Console.WriteLine($"Component {priceComponent.ChargeSetter}_{priceComponent.ChargeMethod}: {priceComponent.Value}");
}

// Simply output the total price
Console.WriteLine($"Total price: currentPredictedPrice.Price.Value")
```

### FlickWebClient

Provides access to your historic usage data, via the flick website.
```
var client = new FlickWebClient("username", "password");

var powerUsageData = await client.GetPowerUsage(DateTime.Now - Timespan.FromDays(7), DateTime.Now - TimeSpan.FromDays(1));

foreach (var bucket in powerUsageData) {
	Console.WriteLine($"ICP Number: {bucket.IcpNumber");
	Console.WriteLine($"Meter Serial Number: {bucket.MeterSerialNumber");
	Console.WriteLine($"ChannelNumber: {bucket.ChannelNumber");
	Console.WriteLine($"StartedAt: {bucket.StartedAt");
	Console.WriteLine($"EndedAt: {bucket.EndedAt");
	Console.WriteLine($"Value: {bucket.Value");
	Console.WriteLine($"UnitCode: {bucket.UnitCode");
	Console.WriteLine($"Status: {bucket.Status");
}
```

## Disclaimer
Access to this data is via Flicks public API's used by its Android App and Website. They're likely to break whenever a significant update is made by Flick.

