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
        private static string RMQ_USER = String.Empty;
        private static string RMQ_PASSWORD = String.Empty;
        private static int RMQ_PORT = 5672;
        private static bool MQ_ENABLED = false;


        public RabbitListener(IHubContext<ChatMessageHub> hubContext, IConfiguration configuration)
        {
            _hubContext = hubContext;
            QUEUE_NAME = configuration["RabbitMq:QueueName"];
            ROUTE_KEY = configuration["RabbitMq:RouteKey"];
            RMQ_HOST = configuration["RabbitMq:Hostname"];
            RMQ_PORT = 5672;
            RMQ_USER = configuration["RabbitMq:User"];
            RMQ_PASSWORD = configuration["RabbitMq:Password"];
            _ = int.TryParse(configuration["RabbitMq:Port"], out RMQ_PORT);
            _ = bool.TryParse(configuration["MessageQueueEnabled"], out MQ_ENABLED);

            if (MQ_ENABLED)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        //HostName = RMQ_HOST,
                        Port = RMQ_PORT,
                        UserName = RMQ_USER,
                        Password = RMQ_PASSWORD,
                        Uri = new Uri(RMQ_HOST)
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RabbitListener init error,ex:{ex.Message}");
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (MQ_ENABLED)
            {
                Register();
            }            
            return Task.CompletedTask;
        }



        // How to process messages
        public virtual bool Process(string message)
        {
            QueueMessage stockInfoMessage = JsonConvert.DeserializeObject<QueueMessage>(message);
            if (stockInfoMessage==null)
            {
                return true;
            }
            string responseMessage = stockInfoMessage.Message;
            User user = stockInfoMessage.User;
            
            if (string.IsNullOrEmpty(responseMessage) || user == null)
            {
                return true;
            }

            if (_hubContext.Clients.Client(user.ConnectionId) == null)
            {
                return true;
            }
            _hubContext.Clients.Client(user.ConnectionId).SendAsync("SendMessage", "stock bot", responseMessage, DateTime.Now.Ticks);
            return true;
        }

        // Registered consumer monitoring here
        public void Register()
        {
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
