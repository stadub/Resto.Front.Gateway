using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Resto.Front.Api.Data.Orders;

namespace alivery
{
    class MessageQueue:IDisposable
    {
        private readonly MessageQueueConfiguration config;
        private readonly IRepository<OrderStatusMessage> messagesRepository;
        IModel channel;
        string queueName;

        public MessageQueue(MessageQueueConfiguration configuration, IRepository<OrderStatusMessage> messagesRepository)
        {
            this.config = configuration;
            this.messagesRepository = messagesRepository;
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

        public void Send(IOrder order)
        {
                var oderId = order.Id.ToString();

                string message = JsonConvert.SerializeObject(order);


            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: new ReadOnlyMemory<byte>(body));

            messagesRepository.Add(new OrderStatusMessage
            {
                Revision = order.Revision,
                OrderId = oderId,
                OrderStatus = (int)order.Status
            });


        }

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}
