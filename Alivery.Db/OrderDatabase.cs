using Alivery.Db.Model;
using SqlBase;

namespace Alivery.Db
{
    //todo: split to 3 db: order, order status, order message to make access operations more atomic
    public class OrderDatabase : SqliteDatabase.SqliteDatabase
    {
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