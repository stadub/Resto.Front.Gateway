using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Alivery.Db.Model;
using SqlBase;
using SQLite;

namespace Alivery.Db
{
    //todo: split to 3 db: order, order status, order message to make access operations more atomic
    [Guid("237c89ab-d689-41c6-b745-10d0926ead99")]
    public class OrderDatabase : SqliteDatabase.SqliteDatabase
    {
        private bool isOpen;

        public OrderDatabase(string databasePath) : base(databasePath, null)
        {
            Order = RegisterTable<Order> ( );
            OrderTransmitStatus = RegisterTable<OrderTransmitStatus> ( );

            //Product = RegisterTable<Product> ();
            OrderStatusMessage = RegisterTable<OrderStatusMessage>();
            //OrderItem = RegisterTable<OrderItem>();

            KitchenOrder = RegisterTable<KitchenOrder>();
            KitchenOrderTransmitStatus = RegisterTable<KitchenOrderTransmitStatus>();
            KitchenOrderStatusMessage = RegisterTable<KitchenOrderStatusMessage>();



        }


        public IRepository<Order> Order { get; }
        public IRepository<OrderTransmitStatus> OrderTransmitStatus { get; }

        //public IRepository<Product> Product { get; }
        public IRepository<OrderStatusMessage> OrderStatusMessage { get; }
        //public IRepository<OrderItem> OrderItem { get; }


        public IRepository<KitchenOrder> KitchenOrder { get; }
        public IRepository<KitchenOrderTransmitStatus> KitchenOrderTransmitStatus { get; }
        public IRepository<KitchenOrderStatusMessage> KitchenOrderStatusMessage { get; }



    }
}