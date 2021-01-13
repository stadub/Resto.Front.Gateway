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
        private readonly IRepository<OrderStatusMessage> msgRepo;
        private readonly IRepository<Order> orderRepo;
        IModel channel;
        string queueName;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        KitchenOrderStatusService kitchenOrderStatusService;
        OrderStatusService orderStatusService;


        public MessageQueue(MessageQueueConfiguration orderConfig, MessageQueueConfiguration korderConfig, OrderDatabase orderDb)
        {
            this.orderConfig = orderConfig;
            this.korderConfig = korderConfig;

            kitchenOrderStatusService = new KitchenOrderStatusService(orderDb);
            orderStatusService = new OrderStatusService(orderDb);
            this.orderDb = orderDb;
            this.msgRepo = orderDb.OrderStatusMessage;
            this.orderRepo = orderDb.Order;
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

        public void SendTest()
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
                channel.QueueDeclare(queue: "hello",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: "hello",
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }


        public void Start(MessageQueueConfiguration config)
        {
           // Main().Wait();
            SendTest();

               ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "test";
            factory.Password = "test";
            //factory.VirtualHost = "/";
            //factory..Protocol = Protocols.FromEnvironment();
            factory.HostName = "185-167-96-77.cloud-xip.io";
            //factory.Port = AmqpTcpEndpoint.UseDefaultPort;
            IConnection conn = factory.CreateConnection();

            string UserName = config.UserName;
            string Password = config.Password;
            string HostName = config.HostName;

            var factory1 = new ConnectionFactory()
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password,

                //HostName = config.HostName,
                //UserName = config.UserName,

                //Password = config.Password,
                VirtualHost = "/",
                Port = 15671// AmqpTcpEndpoint.UseDefaultPort
            };
            queueName = config.QueueName;
            
            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            var properties = channel.CreateBasicProperties();

            properties.Persistent = false;

            byte[] messagebuffer = Encoding.Default.GetBytes("Message from Topic Exchange 'Bombay' ");
            channel.BasicPublish("topic.exchange", "Message.korders.Email", properties, messagebuffer);
            //channel.BasicPublish(queueName, "Message.Bombay.Email", properties, messagebuffer);


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
            await SendOrderStatusUpdatesAsync();
            await SendKitchenOrderStatusUpdatesAsync();
        }
        public async Task SendOrderStatusUpdatesAsync()
        {
            await semaphoreSlim.WaitAsync();

            var orderMessages = await orderStatusService.GetOrderInfoAsync();
            try
            {
                Start(orderConfig);
                foreach (var message in orderMessages)
                {
                    await SendOrderUpdateAsync(message.msg);
                    message.transmitStatus.TransmitStatus=TransmitStatus.Sent;

                    await orderDb.OrderTransmitStatus.UpdateAsync(message.transmitStatus);
                }
               
                Stop();

            }
            catch (Exception e)
            {
                //PluginContext.Log.Error("Error sending status", e);

            }

            semaphoreSlim.Release();
        }

        public async Task SendKitchenOrderStatusUpdatesAsync()
        {
            await semaphoreSlim.WaitAsync();

            var kOrderMessages = await kitchenOrderStatusService.GetKitchenOrderInfoAsync();
            try
            {
                Start(korderConfig);
               
                foreach (var message in kOrderMessages)
                {
                    await SendOrderUpdateAsync(message.msg);
                    message.transmitStatus.TransmitStatus = TransmitStatus.Sent;

                    await orderDb.KitchenOrderTransmitStatus.UpdateAsync(message.transmitStatus);

                }
                Stop();

            }
            catch (Exception e)
            {
                //PluginContext.Log.Error("Error sending status", e);

            }

            semaphoreSlim.Release();
        }


        public async Task SendOrderUpdateAsync<T>(T order) where T: MessageStatusBase
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
            channel?.Dispose();
        }

        public async Task CreateOrderMsg()
        {
            await orderStatusService.CreateOrderInfoAsync();
            await kitchenOrderStatusService.CreateKitchenOrderInfoAsync();
        }
    }
}
