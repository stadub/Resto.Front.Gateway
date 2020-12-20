using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    class CookingPriorityReader
    {
        private readonly ObservableCollection<IOrder> orders = new ObservableCollection<IOrder>();

        public ObservableCollection<IOrder> Orders
        {
            get { return orders; }
        }

        public CookingPriorityReader()
        {

            ReloadOrders(new EntityChangedEventArgs<IOrder>());
        }

        internal void ReloadOrders(Data.Common.EntityChangedEventArgs<IOrder> _)
        {
            orders.Clear();
            PluginContext.Operations.GetOrders()
                .Where(order => order.Status == OrderStatus.New || order.Status == OrderStatus.Bill)
                .Where(order => order.Items.Any(item => !item.Deleted && item.Status != OrderItemStatus.Added))
                .ForEach(orders.Add);
        }


        private void ChangeCookingPriority(IOrder order, int cookingPriority, bool isTopCookingPriority)
        {
            PluginContext.Log.InfoFormat("Changing cooking priority of order {0} to {1} ({2})", order.Number, cookingPriority, isTopCookingPriority ? "vip" : "regular");
            PluginContext.Operations.ChangeCookingPriority(cookingPriority, isTopCookingPriority, order, PluginContext.Operations.GetCredentials());

            // NOTE: performance warning
            // Do not reload all orders every time in a real production code, only replace single changed order.
            // You may get it from ISubmittedEntities by using explicit edit session.
            ReloadOrders(new EntityChangedEventArgs<IOrder>());
        }
    }
}
