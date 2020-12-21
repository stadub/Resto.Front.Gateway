
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Resto.Front.Api;
using Resto.Front.Api.Data.Common;
using Newtonsoft.Json;
using Resto.Front.Api.Data.Orders;

namespace alivery
{
    public class Application:IDisposable
    {

        private ConfigDatabase configDb;
        private OrderDatabase orderDb;
        MessageQueue messageQueue;

        private readonly CompositeDisposable resources = new CompositeDisposable();

        public Application()
        {
            configDb = new ConfigDatabase();
            orderDb = new OrderDatabase();
            configDb.Open();

            var config = new Configurations(configDb.Configuration);

#if DEBUG
            config.Preconfigure();

#endif

            config.OnFirstRun();

            messageQueue = new MessageQueue(config.MessageQueue, orderDb);

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

            PluginContext.Log.Info("CookingPriorityManager started");
            return this;
        }

        private async Task EntryPoint()
        {
            PluginContext.Log.Info("Start init...");


            orderDb.Open();

            await UpdateOrderStatus();

            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            resources.Add(PluginContext.Notifications.OrderChanged
                .Subscribe((x)=>ReceiveFromFront(x).Wait()));

            while (true)
            {
                await Task.Delay(1000);
                if (disposed)
                    break;


                await SendStatusUpdates();
            }
            PluginContext.Log.Info("Exit...");

        }

        private async Task SendStatusUpdates()
        {
            var orders = PluginContext.Operations.GetOrders();
            foreach (var order in orders)
            {
                var oderId = order.Id.ToString();
                var orderTransactionMessages  = await orderDb.OrderStatusMessage.GetAllAsync(x => x.OrderId == oderId);

                if (orderTransactionMessages.Any(x => x.Revision == order.Revision))
                    continue;

                var orderTransactions = await orderDb.Order.GetAllAsync(x => x.OrderId == oderId);

                var initial = orderTransactions.Min(x => x.Revision);



                await orderDb.OrderStatusMessage.AddAsync(new OrderStatusMessage
                {
                    Revision = order.Revision,
                    OrderId = oderId,
                    OrderStatus = (int) order.Status,
                    Status = 0,
                    OrderModelId = null
                });
            }
            await messageQueue.SendStatusUpdatesAsync();

        }

        public async Task UpdateOrderStatus()
        {
            var orders = PluginContext.Operations.GetOrders();

            foreach (IOrder order in orders)
            {
                var oderId = order.Id.ToString();
                var orderTransactions = await orderDb.Order.GetAllAsync(x => x.OrderId == oderId);

                if(orderTransactions.Any(x =>x.Revision == order.Revision ))
                    continue;
                StoreOrder(order);
            }
        }

        internal async Task ReceiveFromFront(EntityChangedEventArgs<IOrder> statusUpdate)
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
            await messageQueue.SendStatusUpdatesAsync();

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
                OrderId = oderId,
                Status = order.Status,
                Json = jsonString
            };
            await orderDb.Order.AddAsync(orderModel);

            await orderDb.OrderStatusMessage.AddAsync(new OrderStatusMessage
            {
                Revision = order.Revision,
                OrderId = oderId,
                OrderStatus = (int)order.Status,
                Status = 0,
                OrderModelId = orderModel.Id
            });
        }

        private bool disposed;


        public void Dispose()
        {
            if (disposed)
                return;
            configDb.Close();
            orderDb.Close();
            messageQueue.Dispose();
            disposed = true;
        }
    }
}
