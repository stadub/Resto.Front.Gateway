using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alivery.Db;
using Alivery.Db.Model;
using Alivery.MessageService;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SqlBase;

namespace alivery
{
    public class MessageQueue:IDisposable
    {
        private readonly ConfigRegistry configRegistry;
        private readonly MessageQueueConfiguration orderConfig;
        private readonly MessageQueueConfiguration korderConfig;

        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        KitchenOrderStatusService kitchenOrderStatusService;
        OrderStatusService orderStatusService;

        public MessageQueue(ConfigRegistry configRegistry, MessageQueueConfiguration orderConfig,
            MessageQueueConfiguration korderConfig)
        {
            this.configRegistry = configRegistry;
            this.orderConfig = orderConfig;
            this.korderConfig = korderConfig;

            kitchenOrderStatusService = new KitchenOrderStatusService();
            orderStatusService = new OrderStatusService();
        }
        
        public (IConnection connection, IModel channel) Start(MessageQueueConfiguration config)
        {
            var userName = config.UserName;
            var password = config.Password;
            var hostName = config.HostName;
            var port = config.Port;

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port,
                VirtualHost = "/"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            
            return (connection, channel);
        }

        public async Task SendStatusUpdatesAsync(OrderDatabase orderDb)
        {

            await SendOrderStatusUpdatesAsync(orderDb);
            await SendKitchenOrderStatusUpdatesAsync(orderDb);
        }

        public async Task SendOrderStatusUpdatesAsync(OrderDatabase orderDb)
        {
            await _semaphoreSlim.WaitAsync();

            var orderMessages = await orderStatusService.GetOrderInfoAsync(orderDb);

            IConnection connection = null;
            IModel channel = null;
            try
            {
                var queueName = orderConfig.QueueName;

                (connection, channel) = Start(orderConfig);

                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                foreach (var message in orderMessages)
                {
                    await SendOrderUpdateAsync(message.msg, queueName, channel);
                    message.transmitStatus.TransmitStatus = TransmitStatus.Sent;

                    await orderDb.OrderTransmitStatus.UpdateAsync(message.transmitStatus);
                }
                
            }
            catch (Exception e)
            {
                //PluginContext.Log.Error("Error sending status", e);

            }
            finally
            {
                channel?.Close();
                connection?.Close();
            }

            _semaphoreSlim.Release();
        }

        public async Task SendKitchenOrderStatusUpdatesAsync(OrderDatabase orderDb)
        {
            await _semaphoreSlim.WaitAsync();

            var kOrderMessages = await kitchenOrderStatusService.GetKitchenOrderInfoAsync(orderDb);

            IConnection connection = null;
            IModel channel = null;
            try
            {
                var queueName = korderConfig.QueueName;

                (connection, channel) = Start(korderConfig);

                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                foreach (var message in kOrderMessages)
                {
                    await SendOrderUpdateAsync(message.msg, queueName, channel);

                    message.transmitStatus.TransmitStatus = TransmitStatus.Sent;

                    await orderDb.KitchenOrderTransmitStatus.UpdateAsync(message.transmitStatus);

                }
            }
            catch (Exception e)
            {
                //PluginContext.Log.Error("Error sending status", e);

            }
            finally
            {
                channel?.Close();
                connection?.Close();
            }
            _semaphoreSlim.Release();
        }


        public async Task SendOrderUpdateAsync<T>(T order, string queueName, IModel channel) where T: MessageStatusBase
        {
            var oderId = order.Id.ToString();

            string message = JsonConvert.SerializeObject(order.Json);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: new ReadOnlyMemory<byte>(body));
        }

        public void Dispose()
        {
        }

        public async Task CreateOrderMsg(OrderDatabase orderDb)
        {

            await orderStatusService.CreateOrderInfoAsync(orderDb);
            await kitchenOrderStatusService.CreateKitchenOrderInfoAsync(orderDb);

        }
    }
}
