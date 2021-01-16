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
        private readonly MessageQueueConfiguration orderConfig;
        private readonly MessageQueueConfiguration korderConfig;
        private readonly OrderDatabase orderDb;

        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        KitchenOrderStatusService kitchenOrderStatusService;
        OrderStatusService orderStatusService;


        public MessageQueue(MessageQueueConfiguration orderConfig, MessageQueueConfiguration korderConfig, OrderDatabase orderDb)
        {
            this.orderConfig = orderConfig;
            this.korderConfig = korderConfig;

            kitchenOrderStatusService = new KitchenOrderStatusService(orderDb);
            orderStatusService = new OrderStatusService(orderDb);
            this.orderDb = orderDb;
        }



        public class Message
        {
            public string Text { get; set; }
        }
        //public static async Task Main()
        //{
        //    var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
        //    {
        //        sbc.Host("rabbitmq://localhost", configurator =>
        //        {
        //            configurator.Password("test");
        //            configurator.Username("test");
        //        });

        //        sbc.ReceiveEndpoint("test_queue", ep =>
        //        {
        //            ep.Handler<Message>(context =>
        //            {
        //                return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
        //            });
        //        });
        //    });

        //    await bus.StartAsync(); // This is important!

        //    await bus.Publish(new Message { Text = "Hi" });

        //    Console.WriteLine("Press any key to exit");
        //    await Task.Run(() => Console.ReadKey());

        //    await bus.StopAsync();
        //}
        //public static async Task Main1()
        //{
        //    var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
        //    {
        //        sbc.Host("185-167-96-77.cloud-xip.io",
        //            configurator =>
        //            {
        //                configurator.Username("test");
        //                configurator.Password("test");
        //                //configurator.UseSsl(sslConfigurator =>
        //                //{
        //                //    sslConfigurator.
        //                //});
        //            });

        //        //sbc.ReceiveEndpoint("test_queue", ep =>
        //        //{
        //        //    ep.Handler<Message>(context =>
        //        //    {
        //        //        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
        //        //    });
        //        //});
        //    });

        //    await bus.StartAsync(); // This is important!

        //    await bus.Publish(new Message { Text = "Hi" });

        //    Console.WriteLine("Press any key to exit");
        //    await Task.Run(() => Console.ReadKey());

        //    await bus.StopAsync();
        //}

        public async Task SendOrderUpdatesAsync<T>(List<T> updates) where T : MessageStatusBase
        {
            var factory = new ConnectionFactory()
            {
                HostName = "185-167-96-77.cloud-xip.io", 
                UserName = "test",
                Password = "test",
                Port = 5672,
                VirtualHost = "/"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "q.front.korders",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                foreach (var update in updates)
                {
                    var oderId = update.Id.ToString();

                    string message = JsonConvert.SerializeObject(update.Json);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                        routingKey: "q.front.korders",
                        basicProperties: null,
                        body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }

            }
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



        public async Task SendStatusUpdatesAsync()
        {
            await SendOrderStatusUpdatesAsync();
            await SendKitchenOrderStatusUpdatesAsync();
        }
        public async Task SendOrderStatusUpdatesAsync()
        {
            await _semaphoreSlim.WaitAsync();

            var orderMessages = await orderStatusService.GetOrderInfoAsync();

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

        public async Task SendKitchenOrderStatusUpdatesAsync()
        {
            await _semaphoreSlim.WaitAsync();

            var kOrderMessages = await kitchenOrderStatusService.GetKitchenOrderInfoAsync();

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

        public async Task CreateOrderMsg()
        {
            await orderStatusService.CreateOrderInfoAsync();
            await kitchenOrderStatusService.CreateKitchenOrderInfoAsync();
        }
    }
}
