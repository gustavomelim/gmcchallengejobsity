using JobsityNetChallenge.Domain;
using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge
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
                await Clients.All.SendAsync("SendMessage", sender, message);
            }
        }

        public async Task ProcessCommand(string sender, string message)
        {
            string responseMessage = string.Empty;
            if (message.StartsWith("/stock="))
            {
                string stockCode = message.Replace("/stock=", "").ToLower();
                var stock = await _stockBotClient.GetStockInfo(stockCode, CancellationToken.None);
                responseMessage = stock != null ? $"{stock.Symbol} quote is ${stock.Close} per share." : "Stock not found.";
            } 
            else
            {
                responseMessage = $"Command [${message}] not recognized !";
            }
            await Clients.Caller.SendAsync("SendMessage", "stock bot", responseMessage);
        }

        public async Task StockCall(string sender, string message)
        {
            var stock = await _stockBotClient.GetStockInfo("aapl.us", CancellationToken.None);
            var stockMessage = stock != null ? $"{stock.Symbol} quote is ${stock.Close} per share." : "Stock not found.";
            await Clients.Caller.SendAsync("SendMessage", sender, stockMessage);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
