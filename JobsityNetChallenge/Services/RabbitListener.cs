using JobsityNetChallenge.Domain;
using JobsityNetChallenge.MessageHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Services
{
    public class RabbitListener : IHostedService
    {
        private readonly IHubContext<ChatMessageHub> _hubContext;

        private readonly IConnection _connection;
        private readonly IModel _channel;

        protected string QUEUE_NAME = "orders";
        protected string ROUTE_KEY = "stockquote";
        private static string RMQ_HOST = "localhost";
        private static int RMQ_PORT = 5672;


        public RabbitListener(IHubContext<ChatMessageHub> hubContext, IConfiguration configuration)
        {
            _hubContext = hubContext;
            QUEUE_NAME = configuration["RabbitMq:QueueName"];
            ROUTE_KEY = configuration["RabbitMq:RouteKey"];
            RMQ_HOST = configuration["RabbitMq:Hostname"];
            RMQ_PORT = 5672;
            _ = int.TryParse(configuration["RabbitMq:Port"], out RMQ_PORT);

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = RMQ_HOST,
                    Port = RMQ_PORT,
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitListener init error,ex:{ex.Message}");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Register();
            return Task.CompletedTask;
        }



        // How to process messages
        public virtual bool Process(string message)
        {
            QueueStockMessage stockInfoMessage = JsonConvert.DeserializeObject<QueueStockMessage>(message);
            if (stockInfoMessage==null)
            {
                return true;
            }
            StockInfo stock = stockInfoMessage.Stock;
            User user = stockInfoMessage.User;
            if (stock == null || user == null)
            {
                return true;
            }

            if (_hubContext.Clients.Client(user.ConnectionId) == null)
            {
                return true;
            }

            string responseMessage = $"{stock.Symbol} quote is ${stock.Close} per share.";
            _hubContext.Clients.Client(user.ConnectionId).SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
            Console.WriteLine($"RabbitListener process:{message}");
            return true;
        }

        // Registered consumer monitoring here
        public void Register()
        {
            Console.WriteLine($"RabbitListener register,routeKey:{ROUTE_KEY}");
            _channel.ExchangeDeclare(exchange: "message", type: "topic");
            _channel.QueueDeclare(queue: QUEUE_NAME, exclusive: false);
            _channel.QueueBind(queue: QUEUE_NAME, exchange: "message", routingKey: ROUTE_KEY);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var result = Process(message);
                if (result)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            _channel.BasicConsume(queue: QUEUE_NAME, consumer: consumer);
        }

        public void DeRegister()
        {
            this._connection.Close();
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._connection.Close();
            return Task.CompletedTask;
        }
    }
}
