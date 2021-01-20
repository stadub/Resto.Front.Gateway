
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
using Utils;
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
        
        ConfigDatabase configDb;
        ConfigRegistry config;

        private readonly CompositeDisposable resources = new CompositeDisposable();

        public Application()
        {
            configDb = new ConfigDatabase("appService.cfg","суперсекретный пароль1");
            configDb.OpenAsync().Wait();

            config = new ConfigRegistry(configDb.Configuration);


            var file = Assembly.GetExecutingAssembly();

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
                try
                {
                    StartMessageService();

                }
                catch (Exception e)
                {
                    PluginContext.Log.Error(e.ToString(),e);
                }

                Thread.Sleep(1000);
            }


            PluginContext.Log.Info("CookingPriorityManager started");
            return this;
        }

        private void StartMessageService()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process process = new Process();
            // Configure the process using the StartInfo properties.

            var path = config.Application.MsgServicePath;

            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = "-n";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (sender, args) =>
            {
                PluginContext.Log.Info(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                PluginContext.Log.Error(args.Data);
            };
            
            process.Start();

            process.WaitForExit();// Waits here for the process to exit.
        }




        private async Task EntryPoint()
        {
            PluginContext.Log.Info("Start init...");


            await UpdateOrderStatus();

            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            resources.Add(PluginContext.Notifications.OrderChanged
                .Subscribe((x)=>
                {
                    try
                    {
                        ReceiveOrderUpdate(x).Wait();
                    }
                    catch (Exception e)
                    {
                        PluginContext.Log.Error(e.ToString(), e);
                    }
                    
                }));


            resources.Add(PluginContext.Notifications.KitchenOrderChanged
                .Subscribe((x) =>
                {
                    try
                    {
                        ReceiveKitchenOrderUpdate(x).Wait();
                    }
                    catch (Exception e)
                    {
                        PluginContext.Log.Error(e.ToString(), e);
                    }
                    
                }));

            while (true)
            {
                await Task.Delay(1000);
                if (disposed)
                    break;

                try
                {
                    await UpdateOrderStatus();
                    await UpdateKOrderStatus();
                }
                catch (Exception e)
                {
                    PluginContext.Log.Error(e.ToString(),e);
                }
               
            }
            PluginContext.Log.Info("Exit...");

        }
        
        public async Task UpdateOrderStatus()
        {
            var orders = PluginContext.Operations.GetOrders();

            var orderDb = new OrderDatabase(config.Application.OrderDbPath);

            //await
                using var conn = await orderDb.OpenAsync();

            foreach (IOrder order in orders)
            {
                var oderId = order.Id.ToString();

                
                var orderTransactions = await orderDb.Order.GetAllAsync(x => x.IikoOrderId == oderId);
                if (orderTransactions.Any(x => x.Revision == order.Revision))
                    continue;


                await StoreOrder(order, orderDb);
            }
        }


        public async Task UpdateKOrderStatus()
        {
            var kOrders = PluginContext.Operations.GetKitchenOrders();

            var orderDb = new OrderDatabase(config.Application.OrderDbPath);

            //await
                using var conn = await orderDb.OpenAsync();

            foreach (var korder in kOrders)
            {
                var oderId = korder.Id.ToString();


                await StoreKitchenOrder(korder, orderDb);
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

            var orderDb = new OrderDatabase(config.Application.OrderDbPath);

            //await 
                using var conn = await orderDb.OpenAsync();
            await StoreOrder(order, orderDb);
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

            var orderDb = new OrderDatabase(config.Application.OrderDbPath);

            //await 
                using var conn = await orderDb.OpenAsync();
            await StoreKitchenOrder(order, orderDb);
        }

        private async Task StoreKitchenOrder(IKitchenOrder order, OrderDatabase orderDb)
        {
            var oderId = order.Id.ToString();

            string jsonString = JsonConvert.SerializeObject(order);

            var orderModel = new KitchenOrder
            {
                CookingPriority = order.CookingPriority,
                Number = (int) order.Number,
                BaseOrderId = order.BaseOrderId.ToString(),
                Json = jsonString,
                IikoOrderId = oderId,
            };


            await orderDb.KitchenOrder.AddAsync(orderModel);

            await orderDb.KitchenOrderTransmitStatus.AddAsync(new KitchenOrderTransmitStatus
            {
                Created = DateTime.Now,
                TransmitStatus = TransmitStatus.Received,
                KitchenOrderId = orderModel.Id,
            });
        }


        private async Task StoreOrder(IOrder order, OrderDatabase orderDb)
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
            //configDb.Close();
            disposed = true;
        }
    }
}
