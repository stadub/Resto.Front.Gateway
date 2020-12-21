using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

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

        public async Task SendStatusUpdatesAsync()
        {
            await semaphoreSlim.WaitAsync();

            var notSent  = await msgRepo.GetAllAsync(x => x.OrderStatus == 0);

            var messages = new List<(Order order, OrderStatusMessage status)>();

            foreach (var status in notSent)
            {
                Order model = await orderRepo.GetByIdAsync(status.OrderModelId);

                if (model == null)
                {
                    model= await orderRepo.FirstAsync(order => order.Revision == status.Revision && order.OrderId == status.OrderId);
                }

                //received state updates during a down time so we have no info
                if (model == null)
                    continue;
                messages.Add((model, status));
            }

            try
            {
                Start();
                foreach (var message in messages)
                {
                    await SendOrderUpdateAsync(message.status, message.order);
                }
                Stop();

            }
            catch (Exception e)
            {
                PluginContext.Log.Error("Error sending status", e);

            }

            semaphoreSlim.Release();
        }


        public async Task SendOrderUpdateAsync(OrderStatusMessage msgStatus, Order order)
        {
            var oderId = order.Id.ToString();

            string message = JsonConvert.SerializeObject(order);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: new ReadOnlyMemory<byte>(body));

            msgStatus.Status = 1;

            await msgRepo.UpsertAsync(msgStatus);

        }

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}
