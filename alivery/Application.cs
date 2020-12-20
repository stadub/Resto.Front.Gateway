
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

            messageQueue = new MessageQueue(config.MessageQueue, orderDb.OrderStatusMessage);

            resources.Add(Disposable.Create(Dispose));
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public Application Start()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();

            PluginContext.Log.Info("CookingPriorityManager started");
            return this;
        }

        private void EntryPoint()
        {
            if (disposed)
                return;

            PluginContext.Log.Info("Start init...");

            orderDb.Open();
            messageQueue.Start();
            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            resources.Add(PluginContext.Notifications.OrderChanged
                .Subscribe(ReceiveFromFront));

            UpdateOrderStatus();
            PluginContext.Log.Info("End init...");

            SendStatusUpdates();


        }

        private void SendStatusUpdates()
        {
            var orders = PluginContext.Operations.GetOrders();
            foreach (var order in orders)
            {
                var oderId = order.Id.ToString();
                var orderTransactionMessages  = orderDb.OrderStatusMessage.GetAll(x => x.OrderId == oderId);

                if (orderTransactionMessages.Any(x => x.Revision == order.Revision))
                    continue;

                var orderTransactions = orderDb.Order.GetAll(x => x.OrderId == oderId);

                var initial = orderTransactions.Min(x => x.Revision);

                messageQueue.Send(order);
            }
        }

        public void UpdateOrderStatus()
        {
            var orders = PluginContext.Operations.GetOrders();

            foreach (IOrder order in orders)
            {
                var oderId = order.Id.ToString();
                var orderTransactions = orderDb.Order.GetAll(x => x.OrderId == oderId);

                if(orderTransactions.Any(x =>x.Revision == order.Revision ))
                    continue;
                StoreOrder(order);
            }
        }

        internal void ReceiveFromFront(EntityChangedEventArgs<IOrder> statusUpdate)
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

            StoreOrder(order);
            messageQueue.Send(order);

        }

        private void StoreOrder(IOrder order)
        {
            var oderId = order.Id.ToString();

            string jsonString = JsonConvert.SerializeObject(order);

            orderDb.Order.Add(new Order
            {
                Revision = order.Revision,
                CloseTime = order.CloseTime,
                OpenTime = order.OpenTime,
                OrderId = oderId,
                Status = order.Status,
                Json = jsonString
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
