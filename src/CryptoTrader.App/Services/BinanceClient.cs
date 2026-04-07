using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoTrader.App.Models;
using Newtonsoft.Json.Linq;

namespace CryptoTrader.App.Services;

public class BinanceClient
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = "https://api.binance.com/api/v3/klines";

    public async Task<List<KlineData>> GetHistoricalDataAsync(string symbol = "BTCUSDT", string interval = "1h", int limit = 500)
    {
        try
        {
            string url = $"{ApiUrl}?symbol={symbol}&interval={interval}&limit={limit}";
            var response = await _httpClient.GetStringAsync(url);
            var jsonArray = JArray.Parse(response);

            var klines = new List<KlineData>();

            foreach (var item in jsonArray)
            {
                klines.Add(new KlineData
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)item[0]).UtcDateTime,
                    Open = (decimal)item[1],
                    High = (decimal)item[2],
                    Low = (decimal)item[3],
                    Close = (decimal)item[4],
                    // Convert Volume (item[5] is string, need to cast appropriately)
                    Volume = decimal.Parse(item[5].ToString()!)
                });
            }

            return klines.OrderBy(x => x.Date).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from Binance: {ex.Message}");
            return new List<KlineData>();
        }
    }
}
