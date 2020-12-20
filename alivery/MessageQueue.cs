using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace alivery
{
    class MessageQueue
    {
        private readonly MessageQueueConfiguration config;
        IModel channel;
        string queueName;

        public MessageQueue(MessageQueueConfiguration configuration)
        {
            this.config = configuration;
        }


        public void Start()
        {
            var factory = new ConnectionFactory()
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost,

            };
            queueName = config.QueueName;

            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        public void Stop()
        {
            channel.Dispose();
        }
        //        string message = "Hello World!";
        public void Send(string message)
        {
            
                

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: queueName,
                    basicProperties: null,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine(" [x] Sent {0}", message);

        }
    }
}
