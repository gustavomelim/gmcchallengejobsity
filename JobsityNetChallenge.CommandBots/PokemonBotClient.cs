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
    public interface IPokemonBotClient : IBotClient
    {
    }

    public class PokemonBotClient : IPokemonBotClient
    {
        private static string API_URL = "https://api.zippopotam.us/us/{0}";
        private readonly HttpClient _httpClient;
        private readonly IMessageProducer _messageProducer;
        private readonly bool _MqEnabled = false;

        public PokemonBotClient(IConfiguration configuration, HttpClient httpClient, IMessageProducer messageProducer)
        {
            _httpClient = httpClient;
            _messageProducer = messageProducer;
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out _MqEnabled);
            API_URL = configuration["PokemonBot:ExternalServiceAPI"];
        }

        public async Task<string> ProcessCommand(User user, string code)
        {
            string responseMessage = $"I will try to fetch [{code}] data, this may take some time !";
            if (_MqEnabled)
            {
                _ = EnqueueInfo(user, code, CancellationToken.None);
            }
            else
            {
                responseMessage = await GetInfo(code, CancellationToken.None);
            }
            return responseMessage;
        }

        public async Task<string> GetInfo(string code, CancellationToken cancellationToken)
        {
            PokemonData remoteData = await PoolDataFromSiteAsJson(code, cancellationToken);
            string info = ParseMessageResult(code, remoteData);
            return info;
        }

        public async Task EnqueueInfo(User user, string code, CancellationToken cancellationToken)
        {
            var message = await GetInfo(code, cancellationToken);
            QueueMessage queueMessage = new QueueMessage()
            {
                Message = message,
                User = user,
            };
            _messageProducer.SendMessage(queueMessage);
        }

        private string ParseMessageResult(string code, PokemonData remoteData)
        {
            string message;
            if (remoteData != null && remoteData.Name != null)
            {
                message = $"Pokemon #{remoteData.Number} is {remoteData.Name}";
            }
            else
            {
                message = $"Could not find information for {code} pokemon.";
            }
            return message;
        }

        private async Task<PokemonData> PoolDataFromSiteAsJson(string code, CancellationToken cancellationToken)
        {
            string url = string.Format(API_URL, code);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            PokemonData result = null;
            try
            {
                result = await response.ToResultAsync<PokemonData>(cancellationToken);
            }
            catch
            {

            }
            return result;
        }



    }
}
