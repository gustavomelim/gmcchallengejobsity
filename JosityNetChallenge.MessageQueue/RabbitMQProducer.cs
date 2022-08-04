using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosityNetChallenge.MessageQueue
{
    public class RabbitMQProducer : IMessageProducer
    {
        private static string QUEUE_NAME = "orders";
        private static string ROUTE_KEY = "stockquote";
        private static string RMQ_HOST = "localhost";
        private static string RMQ_USER = String.Empty;
        private static string RMQ_PASSWORD = String.Empty;
        private static int RMQ_PORT = 5672;

        public RabbitMQProducer(IConfiguration configuration)
        {
            QUEUE_NAME = configuration["RabbitMq:QueueName"];
            ROUTE_KEY = configuration["RabbitMq:RouteKey"];
            RMQ_HOST = configuration["RabbitMq:Hostname"];
            RMQ_PORT = 5672;
            RMQ_USER = configuration["RabbitMq:User"];
            RMQ_PASSWORD = configuration["RabbitMq:Password"];
            _ = int.TryParse(configuration["RabbitMq:Port"], out RMQ_PORT);
        }

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                //HostName = RMQ_HOST,
                Port = RMQ_PORT,
                UserName = RMQ_USER,
                Password = RMQ_PASSWORD,
                Uri = new Uri(RMQ_HOST)
            };

            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: QUEUE_NAME,
                                        durable: false,
                                        exclusive: false,
                                        arguments: null);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "message",
                                    routingKey: ROUTE_KEY,
                                    basicProperties: null,
                                    body: body);

        }


    }
}

