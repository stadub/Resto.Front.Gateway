
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Alivery.Db;
using Alivery.Db.Model;
using DbConfiguration;
using Resto.Front.Api;
using Resto.Front.Api.Data.Common;
using Newtonsoft.Json;
using Resto.Front.Api.Data.Kitchen;
using Resto.Front.Api.Data.Orders;
using OrderStatus = Alivery.Db.Model.OrderStatus;

namespace alivery
{
    public class Application:IDisposable
    {

        private ConfigDatabase configDb;
        private OrderDatabase orderDb;

        private readonly CompositeDisposable resources = new CompositeDisposable();

        public Application()
        {
            configDb = new ConfigDatabase("суперсекретный пароль");
            orderDb = new OrderDatabase();
            configDb.Open();

            var config = new ConfigRegistry(configDb.Configuration);

            var file = System.Reflection.Assembly.GetExecutingAssembly();

            config.SyncFromConfigFile(file.Location);
            
            resources.Add(Disposable.Create(Dispose));
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public Application Start()
        {
            var windowThread = new Thread(() =>
            {
                EntryPoint().Wait();
            });



            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();

            //restart messaging service in case of error
            while (!disposed)
            {
                StartMessageService();
            }


            PluginContext.Log.Info("CookingPriorityManager started");
            return this;
        }

        private void StartMessageService()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = $"{assemblyPath}\\service\\Alivery.MessageService.exe";
            process.StartInfo.Arguments = "-n";
            //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();// Waits here for the process to exit.
        }

        private async Task EntryPoint()
        {
            PluginContext.Log.Info("Start init...");


            orderDb.Open();

            await UpdateOrderStatus();

            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            resources.Add(PluginContext.Notifications.OrderChanged
                .Subscribe((x)=>ReceiveOrderUpdate(x).Wait()));


            resources.Add(PluginContext.Notifications.KitchenOrderChanged
                .Subscribe((x) => ReceiveKitchenOrderUpdate(x).Wait()));

            while (true)
            {
                await Task.Delay(1000);
                if (disposed)
                    break;


                await UpdateOrderStatus();
            }
            PluginContext.Log.Info("Exit...");

        }

        public async Task UpdateOrderStatus()
        {
            var orders = PluginContext.Operations.GetOrders();

            foreach (IOrder order in orders)
            {
                var oderId = order.Id.ToString();
                var orderTransactions = await orderDb.Order.GetAllAsync(x => x.IikoOrderId == oderId);

                if(orderTransactions.Any(x =>x.Revision == order.Revision ))
                    continue;
                await StoreOrder(order);
            }
        }

        internal async Task ReceiveOrderUpdate(EntityChangedEventArgs<IOrder> statusUpdate)
        {
            var order = statusUpdate.Entity;

            switch (statusUpdate.EventType)
            {
                case EntityEventType.Created:
                    break;
                case EntityEventType.Updated:
                    break;
                case EntityEventType.Removed:
                    break;
            }

            await StoreOrder(order);

        }
        private async Task ReceiveKitchenOrderUpdate(EntityChangedEventArgs<IKitchenOrder> statusUpdate)
        {
            var order = statusUpdate.Entity;

            switch (statusUpdate.EventType)
            {
                case EntityEventType.Created:
                    break;
                case EntityEventType.Updated:
                    break;
                case EntityEventType.Removed:
                    break;
            }

            await StoreKitchenOrder(order);
        }

        private async Task StoreKitchenOrder(IKitchenOrder order)
        {
            var oderId = order.Id.ToString();

            string jsonString = JsonConvert.SerializeObject(order);

            var orderModel = new KitchenOrder
            {
                CookingPriority = order.CookingPriority,
                Number = (int)order.Number,
                BaseOrderId = order.BaseOrderId.ToString(),
                Json = jsonString
            };
            await orderDb.KitchenOrder.AddAsync(orderModel);

            await orderDb.KitchenOrderTransmitStatus.AddAsync(new KitchenOrderTransmitStatus
            {
                Created = DateTime.Now,
                TransmitStatus = TransmitStatus.Received,
                KitchenOrderId = orderModel.Id
            });
        }

        private async Task StoreOrder(IOrder order)
        {
            var oderId = order.Id.ToString();

            string jsonString = JsonConvert.SerializeObject(order);

            var orderModel = new Order
            {
                Revision = order.Revision,
                CloseTime = order.CloseTime,
                OpenTime = order.OpenTime,
                IikoOrderId = oderId,
                OrderStatus = (OrderStatus) order.Status,
                Json = jsonString
            };
            await orderDb.Order.AddAsync(orderModel);

            await orderDb.OrderTransmitStatus.AddAsync(new OrderTransmitStatus
            {
                Created = DateTime.Now,
                TransmitStatus = TransmitStatus.Received,
                OrderId = orderModel.Id
            });
        }

        private bool disposed;


        public void Dispose()
        {
            if (disposed)
                return;
            configDb.Close();
            orderDb.Close();
            disposed = true;
        }
    }
}
