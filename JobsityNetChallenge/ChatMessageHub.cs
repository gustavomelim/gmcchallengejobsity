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
            Message storeMessage = new Message { Id = DateTime.Now.Ticks, MessageContent = message, User = sender};
            _chatStorage.SaveMessage(storeMessage);
            await Clients.All.SendAsync("SendMessage", sender, message);
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
