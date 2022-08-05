using JobsityNetChallenge.CommandBots;
using JobsityNetChallenge.Domain;
using JobsityNetChallenge.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        private readonly IPokemonBotClient _pkmBotClient;
        private readonly IChatStorage _chatStorage;
        private readonly bool _MqEnabled = false;
        private readonly string commandParner = @"^/(.*)=(.*)";
        private readonly Regex _commandRegularExpression;
        private readonly Dictionary<string, IBotClient> _botCommands = new Dictionary<string, IBotClient>();

        public ChatMessageHub(IStockBotClient stockBotClient, IZipBotClient zipBotClient, IPokemonBotClient pkmBotClient, IChatStorage chatStorage, IConfiguration configuration)
        {
            _stockBotClient = stockBotClient;
            _zipBotClient = zipBotClient;
            _pkmBotClient = pkmBotClient;
            _chatStorage = chatStorage;
            _commandRegularExpression = new Regex(commandParner);
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out _MqEnabled);
            _botCommands.Add("stock", _stockBotClient);
            _botCommands.Add("zip", _zipBotClient);
            _botCommands.Add("pkm", _pkmBotClient);
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

            string responseMessage = $"Command [{command}] not recognized !";
            var commandImplKey = _botCommands.Keys.FirstOrDefault(x => x.Equals(command, StringComparison.InvariantCultureIgnoreCase));
            if (commandImplKey != null)
            {
                responseMessage = await _botCommands[commandImplKey].ProcessCommand(user, commandValue);
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
            var messages = _chatStorage.LoadLastMessages(50).OrderBy(x => x.Id);
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
