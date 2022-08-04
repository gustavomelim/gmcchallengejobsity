using JobsityNetChallenge.Domain;
using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using JobsityNetChallenge.ZipBot;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.MessageHub
{
    public class ChatMessageHub : Hub
    {
        private readonly IStockBotClient _stockBotClient;
        private readonly IZipBotClient _zipBotClient;
        private readonly IChatStorage _chatStorage;
        private readonly bool _MqEnabled = false;
        private readonly string commandParner = @"^/(.*)=(.*)";
        private readonly Regex _commandRegularExpression;

        public ChatMessageHub(IStockBotClient stockBotClient, IZipBotClient zipBotClient, IChatStorage chatStorage, IConfiguration configuration)
        {
            _stockBotClient = stockBotClient;
            _zipBotClient = zipBotClient;
            _chatStorage = chatStorage;
            _commandRegularExpression = new Regex(commandParner);
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out _MqEnabled);
        }

        public async Task SendMessage(string sender, string message)
        {
            string userId = RetrieveUserId();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Match match = _commandRegularExpression.Match(message.ToLower());
            if (match.Success)
            {
                await ProcessCommandImp(userId, match.Groups[1].Value, match.Groups[2].Value);
            }
            else
            {
                Message storeMessage = new Message { Id = DateTime.Now.Ticks, MessageContent = message, User = sender };
                _chatStorage.SaveMessage(storeMessage);
                await Clients.All.SendAsync("SendMessage", sender, message, storeMessage.Id);
            }
        }

        public async Task ProcessCommandImp(string userId, string command, string commandValue)
        {
            var user = _chatStorage.FetchUser(userId);
            if (user == null)
            {
                return;
            }

            if (command.Equals("stock", StringComparison.InvariantCultureIgnoreCase))
            {
                await ProcessStockCommand(user, commandValue);
            } else if (command.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
            {
                await ProcessZipCommand(user, commandValue);
            }
            else
            {
                string responseMessage = $"Command [{command}] not recognized !";
                await Clients.Caller.SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
            }
        }

        public async Task ProcessStockCommand(User user, string stockCode)
        {
            string responseMessage = $"I will try to fetch [{stockCode}] data, this may take some time !";
            if (_MqEnabled)
            {
                _ = _stockBotClient.EnqueueStockInfo(user, stockCode, CancellationToken.None);
            }
            else
            {
                responseMessage = await _stockBotClient.GetStockInfo(stockCode, CancellationToken.None);
            }
            await Clients.Caller.SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
        }

        public async Task ProcessZipCommand(User user, string zipCode)
        {
            string responseMessage = $"I will try to fetch [{zipCode}] data, this may take some time !";
            if (_MqEnabled)
            {
                _ = _zipBotClient.EnqueueZipInfo(user, zipCode, CancellationToken.None);
            }
            else
            {
                responseMessage = await _zipBotClient.GetZipInfo(zipCode, CancellationToken.None);
            }
            await Clients.Caller.SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
        }


        public override async Task OnConnectedAsync()
        {
            string userId = RetrieveUserId();
            var user = _chatStorage.FetchUser(userId);
            if (user != null)
            {
                user.ConnectionId = Context.ConnectionId;
                _chatStorage.SaveUser(user);
            }
            await Clients.All.SendAsync("UserConnected", userId);
            var messages = _chatStorage.LoadLastMessages(50).OrderBy(x=>x.Id);
            await Clients.Caller.SendAsync("SendHistory", messages);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userId = RetrieveUserId();
            await Clients.All.SendAsync("UserDisconnected", userId);
            await base.OnDisconnectedAsync(ex);
        }

        private string RetrieveUserId()
        {
            var accessToken = Context.GetHttpContext().Request.Query["access_token"];
            return accessToken;    
        }
    }
}
