using CsvHelper;
using CsvHelper.Configuration;
using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Extensions;
using JosityNetChallenge.MessageQueue;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.StockBot
{
    public interface IStockBotClient
    {
        Task EnqueueStockInfo(User user, string stockCode, CancellationToken cancellationToken);
        Task<string> GetStockInfo(string stockCode, CancellationToken cancellationToken);
    }

    public class StockBotClient : IStockBotClient
    {
        private static string STOCK_URL = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e={1}";
        private readonly HttpClient _httpClient;
        private readonly IMessageProducer _messageProducer;


        public StockBotClient(IConfiguration configuration, HttpClient httpClient, IMessageProducer messageProducer)
        {
            _httpClient = httpClient;
            _messageProducer = messageProducer;
            STOCK_URL = configuration["StockBot:ExternalServiceAPI"];
        }

        public async Task<string> GetStockInfo(string stockCode, CancellationToken cancellationToken)
        {
            StockInfoList remoteDate = await PoolDataFromStocksSiteAsCSV(stockCode, cancellationToken);
            StockInfo stockInfo = remoteDate?.Symbols?.FirstOrDefault(x => string.Equals(x.Symbol,stockCode, StringComparison.InvariantCultureIgnoreCase));
            string message = ParseMessageResult(stockCode, stockInfo);
            return message;
        }

        public async Task EnqueueStockInfo(User user, string stockCode, CancellationToken cancellationToken)
        {
            StockInfoList remoteDate = await PoolDataFromStocksSiteAsCSV(stockCode, cancellationToken);
            StockInfo stockInfo = remoteDate?.Symbols?.FirstOrDefault(x => string.Equals(x.Symbol, stockCode, StringComparison.InvariantCultureIgnoreCase));
            string message = ParseMessageResult(stockCode, stockInfo);
            QueueStockMessage queueStockMessage = new QueueStockMessage()
            {
                Message = message,
                User = user,
            };
            _messageProducer.SendMessage(queueStockMessage);
        }

        private string ParseMessageResult(string stockSymbol, StockInfo stock)
        {
            string message;

            if (stock != null && !stock.HasError && stock.Symbol.Equals(stockSymbol,StringComparison.InvariantCultureIgnoreCase))
            {
                message = $"{stock.Symbol} quote is ${stock.Open} per share.";
            }
            else
            {
                message = $"Could not find information for {stockSymbol} quote.";
            }
            return message;
        }

        private async Task<StockInfoList> PoolDataFromStocksSiteAsCSV(string stockCode, CancellationToken cancellationToken)
        {
            string url = string.Format(STOCK_URL, stockCode, "csv");
            var response = await _httpClient.GetAsync(url, cancellationToken);
            string responseContent = await response.ToResultAsStringAsync(cancellationToken);
            StockInfoList result = ParseStockCSVInfoUsingLibrary(responseContent);
            return result;
        }

        private StockInfoList ParseStockCSVInfoUsingLibrary(string dataAsCSV)
        {
            StockInfoList result = new StockInfoList();
            result.Symbols = new List<StockInfo>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using (var reader = new StringReader(dataAsCSV))
            using (var csv = new CsvReader(reader, config))
            {
                try
                {
                    csv.Context.RegisterClassMap<StockInfoWithConverter>();
                    var records = csv.GetRecords<StockInfo>().ToList();
                    result.Symbols.AddRange(records);
                } catch (Exception)
                {

                }
            }
            return result;
        }

        private StockInfoList ParseStockCSVInfo(string dataAsCSV)
        {
            StockInfoList result = new StockInfoList();
            result.Symbols = new List<StockInfo>();
            string[] lines = dataAsCSV.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            StockInfo stockInfo;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] content = lines[i].Split(",", StringSplitOptions.TrimEntries);
                if (lines[i].Contains("N/D,N/D,N/D,N/D,N/D,N/D,N/D",StringComparison.InvariantCultureIgnoreCase))
                {
                    stockInfo = new StockInfo()
                    {
                        Symbol = content[0],
                    };
                } 
                else
                {
                    stockInfo = new StockInfo()
                    {
                        Symbol = content[0],
                        Date = ManualCSVValidator.ParseStockDate(content[1]),
                        Time = ManualCSVValidator.ParseStockTime(content[2]),
                        Open = ManualCSVValidator.ParseStockValue(content[3]),
                        High = ManualCSVValidator.ParseStockValue(content[4]),
                        Low = ManualCSVValidator.ParseStockValue(content[5]),
                        Close = ManualCSVValidator.ParseStockValue(content[6]),
                        Volume = ManualCSVValidator.ParseStockVolume(content[7]),
                    };
                }                
                result.Symbols.Add(stockInfo);
            }
            return result;
        }

    }
}
