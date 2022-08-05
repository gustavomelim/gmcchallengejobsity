using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Domain.Extensions;
using JosityNetChallenge.MessageQueue;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.CommandBots
{
    public interface IZipBotClient : IBotClient
    {

    }



    public class ZipBotClient : IZipBotClient
    {
        private static string API_URL = "https://api.zippopotam.us/us/{0}";
        private readonly HttpClient _httpClient;
        private readonly IMessageProducer _messageProducer;
        private readonly bool _MqEnabled = false;


        public ZipBotClient(IConfiguration configuration, HttpClient httpClient, IMessageProducer messageProducer)
        {
            _httpClient = httpClient;
            _messageProducer = messageProducer;
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out _MqEnabled);
            API_URL = configuration["ZipBot:ExternalServiceAPI"];
        }

        public async Task<string> ProcessCommand(User user, string code)
        {
            string responseMessage = $"I will try to fetch [{code}] data, this may take some time !";
            if (_MqEnabled)
            {
                _ = EnqueueZipInfo(user, code, CancellationToken.None);
            }
            else
            {
                responseMessage = await GetZipInfo(code, CancellationToken.None);
            }
            return responseMessage;
        }

        public async Task<string> GetZipInfo(string zipCode, CancellationToken cancellationToken)
        {
            ZipInfo remoteData = await PoolDataFromSiteAsJson(zipCode, cancellationToken);
            string zipInfo = ParseMessageResult(zipCode, remoteData);
            return zipInfo;
        }

        public async Task EnqueueZipInfo(User user, string zipCode, CancellationToken cancellationToken)
        {
            var message = await GetZipInfo(zipCode, cancellationToken);
            QueueStockMessage queueStockMessage = new QueueStockMessage()
            {
                Message = message,
                User = user,
            };
            _messageProducer.SendMessage(queueStockMessage);
        }

        private string ParseMessageResult(string zipCode, ZipInfo remoteData)
        {
            string message;
            if (remoteData != null && remoteData.Places != null && remoteData.Places.Any())
            {
                message = $"Zip code {remoteData.PostCode} is located at {remoteData.Places.FirstOrDefault()?.PlaceName} - {remoteData.Places.FirstOrDefault()?.State}";
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
            ZipInfo result = null;
            try
            {
                result = await response.ToResultAsync<ZipInfo>(cancellationToken);
            } 
            catch
            {

            }            
            return result;
        }
    }
}
