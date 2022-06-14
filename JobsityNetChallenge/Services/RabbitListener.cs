using JobsityNetChallenge.Domain;
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

        private readonly IConnection connection;
        private readonly IModel channel;

        protected string RouteKey;
        protected string QueueName;

        public RabbitListener()
        {
            QueueName = "orders";
            RouteKey = "stockquote";
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost"
                };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
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
            StockInfo stockInfo = JsonConvert.DeserializeObject<StockInfo>(message);
            
            Console.WriteLine($"RabbitListener process:{message}");
            return true;
        }

        // Registered consumer monitoring here
        public void Register()
        {
            Console.WriteLine($"RabbitListener register,routeKey:{RouteKey}");
            channel.ExchangeDeclare(exchange: "message", type: "topic");
            channel.QueueDeclare(queue: QueueName, exclusive: false);
            channel.QueueBind(queue: QueueName, exchange: "message", routingKey: RouteKey);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var result = Process(message);
                if (result)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            channel.BasicConsume(queue: QueueName, consumer: consumer);
        }

        public void DeRegister()
        {
            this.connection.Close();
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.connection.Close();
            return Task.CompletedTask;
        }
    }
}
