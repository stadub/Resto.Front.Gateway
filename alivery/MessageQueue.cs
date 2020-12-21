using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Resto.Front.Api;
using Resto.Front.Api.Data.Orders;

namespace alivery
{
    class MessageQueue:IDisposable
    {
        private readonly MessageQueueConfiguration config;
        private readonly OrderDatabase orderDb;
        private readonly IRepository<OrderStatusMessage> msgRepo;
        private readonly IRepository<Order> orderRepo;
        IModel channel;
        string queueName;

        public MessageQueue(MessageQueueConfiguration configuration, OrderDatabase orderDb)
        {
            this.config = configuration;
            this.orderDb = orderDb;
            this.msgRepo = orderDb.OrderStatusMessage;
            this.orderRepo = orderDb.Order;
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendStatusUpdates()
        {
            Start();

            var notSent  = msgRepo.GetAll(x => x.OrderStatus == 0);

            foreach (var orderStatusMessage in notSent)
            {
                Order model = orderRepo.GetById(orderStatusMessage.OrderModelId);

                if (model == null)
                {
                    model= orderRepo.First(order => order.Revision == orderStatusMessage.Revision && order.OrderId == orderStatusMessage.OrderId);
                }

                //received state updates during a down time so we have no info
                if (model == null)
                    continue;
                try
                {
                    SendOrderUpdate(orderStatusMessage, model);

                }
                catch (Exception e)
                {
                    PluginContext.Log.Error("Error sending status", e);

                }
            }

            Stop();

        }


        public void SendOrderUpdate(OrderStatusMessage msgStatus, Order order)
        {
            Start();
            var oderId = order.Id.ToString();

            string message = JsonConvert.SerializeObject(order);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: new ReadOnlyMemory<byte>(body));

            msgStatus.Status = 1;

            msgRepo.Upsert(msgStatus);

        }

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}
