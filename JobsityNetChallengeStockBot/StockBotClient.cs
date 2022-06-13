﻿using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.StockBot
{
    public interface IStockBotClient
    {
        Task<StockInfo> GetStockInfo(string stockCode, CancellationToken cancellationToken);
    }

    public class StockBotClient : IStockBotClient
    {
        private static CultureInfo GLOBAL_CULTURE = CultureInfo.InvariantCulture;
        private static DateTimeStyles GLOBAL_DATE_STYLE = DateTimeStyles.None;

        private static string STOCK_URL = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e={1}";
        private readonly HttpClient _httpClient;

        public StockBotClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StockInfo> GetStockInfo(string stockCode, CancellationToken cancellationToken)
        {
            StockInfoList remoteDate = await PoolDataFromStocksSiteAsCSV(stockCode, cancellationToken);
            StockInfo stockInfo = remoteDate?.Symbols?.FirstOrDefault(x => string.Equals(x.Symbol,stockCode, StringComparison.InvariantCultureIgnoreCase));
            return stockInfo;
        }

        private async Task<StockInfoList> PoolDataFromStocksSiteAsJson(string stockCode, CancellationToken cancellationToken)
        {
            string url = string.Format(STOCK_URL, stockCode, "json");
            StockInfoList result;

            var response = await _httpClient.GetAsync(url, cancellationToken);
            result = await response.ToResultAsync<StockInfoList>(cancellationToken);
            return result;
        }

        private async Task<StockInfoList> PoolDataFromStocksSiteAsCSV(string stockCode, CancellationToken cancellationToken)
        {
            string url = string.Format(STOCK_URL, stockCode, "csv");
            var response = await _httpClient.GetAsync(url, cancellationToken);
            string responseContent = await response.ToResultAsStringAsync(cancellationToken);
            StockInfoList result = ParseStockCSVInfo(responseContent);
            return result;
        }

        private StockInfoList ParseStockCSVInfo(string dataAsCSV)
        {
            StockInfoList result = new StockInfoList();
            result.Symbols = new List<StockInfo>();
            string[] lines = dataAsCSV.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] content = lines[i].Split(",", StringSplitOptions.TrimEntries);
                result.Symbols.Add(new StockInfo()
                {
                    Symbol = content[0],
                    Date = ParseStockDate(content[1]),
                    Time = ParseStockTime(content[2]),
                    Open = ParseStockValue(content[3]),
                    High = ParseStockValue(content[4]),
                    Low = ParseStockValue(content[5]),
                    Close = ParseStockValue(content[6]),
                    Volume = ParseStockVolume(content[7]),
                });
            }
            return result;
        }

        private static DateTime ParseStockDate(string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "yyyy-MM-dd", GLOBAL_CULTURE, GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        private static DateTime ParseStockTime(string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "HH:mm:ss", GLOBAL_CULTURE, GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        private static decimal ParseStockValue(string data)
        {
            decimal result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                decimal.TryParse(data, NumberStyles.Float, GLOBAL_CULTURE, out result);
            }
            return result;
        }

        private static long ParseStockVolume(string data)
        {
            int result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                int.TryParse(data, NumberStyles.Float, GLOBAL_CULTURE, out result);
            }
            return result;
        }
    }
}