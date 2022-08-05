using CsvHelper;
using CsvHelper.Configuration;
using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Extensions;
using JobsityNetChallenge.Domain.Utils;
using JosityNetChallenge.MessageQueue;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace JobsityNetChallenge.CommandBots
{
    public interface IStockBotClient : IBotClient
    {

    }

    public class StockBotClient : IStockBotClient
    {
        private static string STOCK_URL = "https://stooq.com/q/l/?s={0}&f=sd2t2ohlcv&h&e={1}";
        private readonly HttpClient _httpClient;
        private readonly IMessageProducer _messageProducer;
        private readonly bool _MqEnabled = false;


        public StockBotClient(IConfiguration configuration, HttpClient httpClient, IMessageProducer messageProducer)
        {
            _httpClient = httpClient;
            _messageProducer = messageProducer;
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out _MqEnabled);
            STOCK_URL = configuration["StockBot:ExternalServiceAPI"];
        }

        public async Task<string> ProcessCommand(User user, string code)
        {
            string responseMessage = $"I will try to fetch [{code}] data, this may take some time !";
            if (_MqEnabled)
            {
                _ = EnqueueStockInfo(user, code, CancellationToken.None);
            }
            else
            {
                responseMessage = await GetStockInfo(code, CancellationToken.None);
            }
            return responseMessage;
        }

        public async Task<string> GetStockInfo(string stockCode, CancellationToken cancellationToken)
        {
            StockInfoList remoteDate = await PoolDataFromStocksSiteAsCSV(stockCode, cancellationToken);
            StockInfo stockInfo = remoteDate.Symbols.FirstOrDefault(x => string.Equals(x.Symbol, stockCode, StringComparison.InvariantCultureIgnoreCase));
            string message = ParseMessageResult(stockCode, stockInfo);
            return message;
        }

        public async Task EnqueueStockInfo(User user, string stockCode, CancellationToken cancellationToken)
        {
            string message = await GetStockInfo(stockCode, cancellationToken);
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

            if (stock != null && !stock.HasError && stock.Symbol.Equals(stockSymbol, StringComparison.InvariantCultureIgnoreCase))
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
                }
                catch (Exception)
                {

                }
            }
            return result;
        }
    }
}
