using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Extensions;
using JosityNetChallenge.MessageQueue;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.ZipBot
{
    public interface IZipBotClient
    {
        Task EnqueueZipInfo(User user, string stockCode, CancellationToken cancellationToken);
        Task<string> GetZipInfo(string stockCode, CancellationToken cancellationToken);
    }



    public class ZipBotClient : IZipBotClient
    {
        private static string API_URL = "https://api.zippopotam.us/us/{0}";
        private readonly HttpClient _httpClient;
        private readonly IMessageProducer _messageProducer;


        public ZipBotClient(IConfiguration configuration, HttpClient httpClient, IMessageProducer messageProducer)
        {
            _httpClient = httpClient;
            _messageProducer = messageProducer;
            API_URL = configuration["ZipBot:ExternalServiceAPI"];
        }

        public async Task<string> GetZipInfo(string zipCode, CancellationToken cancellationToken)
        {
            ZipInfo remoteData = await PoolDataFromSiteAsJson(zipCode, cancellationToken);
            string zipInfo = ParseMessageResult(zipCode, remoteData);
            return zipInfo;
        }

        public async Task EnqueueZipInfo(User user, string zipCode, CancellationToken cancellationToken)
        {
            ZipInfo remoteData = await PoolDataFromSiteAsJson(zipCode, cancellationToken);
            QueueStockMessage queueStockMessage = new QueueStockMessage()
            {
                Message = ParseMessageResult(zipCode, remoteData),
                User = user,
            };
            _messageProducer.SendMessage(queueStockMessage);
        }

        private string ParseMessageResult(string zipCode, ZipInfo remoteData)
        {
            string message;
            if (remoteData != null && remoteData.Places != null && remoteData.Places.Any())
            {
                message = $"Zip code {remoteData.PostCode} is related to {remoteData.Places.FirstOrDefault()?.PlaceName}@{remoteData.Places.FirstOrDefault()?.State}";
            }
            else
            {
                message = $"Could not find information for {zipCode} zipCode.";
            }
            return message;            
        }

        private async Task<ZipInfo> PoolDataFromSiteAsJson(string zipCode, CancellationToken cancellationToken)
        {
            string url = string.Format(API_URL, zipCode);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            ZipInfo result = await response.ToResultAsync<ZipInfo>(cancellationToken);
            return result;
        }
    }
}
