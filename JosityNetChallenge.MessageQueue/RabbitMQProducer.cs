using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosityNetChallenge.MessageQueue
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message);
    }
    public class RabbitMQProducer : IMessageProducer
    {
        private static string QUEUE = "orders";

        public void SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(QUEUE);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: "", routingKey: QUEUE, body: body);
        }
    }
}
