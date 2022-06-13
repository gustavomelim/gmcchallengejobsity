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
                await ProcessCommand(sender, message);
            }
            else
            {
                Message storeMessage = new Message { Id = DateTime.Now.Ticks, MessageContent = message, User = sender };
                _chatStorage.SaveMessage(storeMessage);
                await Clients.All.SendAsync("SendMessage", sender, message, storeMessage.Id);
            }
        }

        public async Task ProcessCommand(string sender, string message)
        {
            string responseMessage = string.Empty;
            if (message.StartsWith("/stock="))
            {
                string stockCode = message.Replace("/stock=", "").ToLower();
                var stock = await _stockBotClient.GetStockInfo(stockCode, CancellationToken.None);
                responseMessage = stock != null && !string.IsNullOrEmpty(stock.Symbol)  ? $"{stock.Symbol} quote is ${stock.Close} per share." : $"Stock [{stockCode}] not found.";
            }
            else
            {
                responseMessage = $"Command [${message}] not recognized !";
            }
            await Clients.Caller.SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
        }

        public async Task StockCall(string sender, string message)
        {
            var stock = await _stockBotClient.GetStockInfo("aapl.us", CancellationToken.None);
            var stockMessage = stock != null ? $"{stock.Symbol} quote is ${stock.Close} per share." : "Stock not found.";
            await Clients.Caller.SendAsync("SendMessage", sender, stockMessage);
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
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }

        private string RetrieveUserId()
        {
            var accessToken = Context.GetHttpContext().Request.Query["access_token"];
            return accessToken;    
        }

        private void RetrieveHeaders ()
        {
            var httpCtx = Context.GetHttpContext();
            var someHeaderValue = httpCtx.Request.Headers["Foo"].ToString();
            var allHeaders = httpCtx.Request.Headers.Values;

        }
    }
}
