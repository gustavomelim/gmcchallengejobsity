using JobsityNetChallenge.Domain;
using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.MessageHub
{
    public class ChatMessageHub : Hub
    {
        private readonly IStockBotClient _stockBotClient;
        private readonly IChatStorage _chatStorage;
        public ChatMessageHub(IStockBotClient stockBotClient, IChatStorage chatStorage)
        {
            _stockBotClient = stockBotClient;
            _chatStorage = chatStorage;
        }

        public async Task SendMessage(string sender, string message)
        {
            string userId = RetrieveUserId();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            if (message.StartsWith("/") && !message.StartsWith("/ "))
            {
                await ProcessCommand(userId, message);
            }
            else
            {
                Message storeMessage = new Message { Id = DateTime.Now.Ticks, MessageContent = message, User = sender };
                _chatStorage.SaveMessage(storeMessage);
                await Clients.All.SendAsync("SendMessage", sender, message, storeMessage.Id);
            }
        }

        public async Task ProcessCommand(string userId, string message)
        {
            var user = _chatStorage.FetchUser(userId);
            if (user == null)
            {
                return;
            }

            string responseMessage;
            if (message.StartsWith("/stock="))
            {
                string stockCode = message.Replace("/stock=", "").ToLower();
                _ = _stockBotClient.EnqueueStockInfo(user, stockCode, CancellationToken.None);
                responseMessage = $"I will try to fetch [{stockCode}] data, this may take some time !";
                //responseMessage = stock != null && !string.IsNullOrEmpty(stock.Symbol)  ? $"{stock.Symbol} quote is ${stock.Close} per share." : $"Stock [{stockCode}] not found.";
            }
            else
            {
                responseMessage = $"Command [{message}] not recognized !";
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
