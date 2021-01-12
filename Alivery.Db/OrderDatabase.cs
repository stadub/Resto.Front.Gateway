using Alivery.Db.Model;
using SqliteDatabase;

namespace Alivery.Db
{
    public class OrderDatabase : DatabaseBase
    {
        public OrderDatabase() : base("alivery", "Сорок два!")
        {
            Order = RegisterTable<Order> ( );
            //Product = RegisterTable<Product> ();
            OrderStatusMessage = RegisterTable<OrderStatusMessage>();
            //OrderItem = RegisterTable<OrderItem>();

            KitchenOrder = RegisterTable<KitchenOrder>();
            KitchenOrderStatusMessage = RegisterTable<KitchenOrderStatusMessage>();
        }

        public IRepository<Order> Order { get; }
        //public IRepository<Product> Product { get; }
        public IRepository<OrderStatusMessage> OrderStatusMessage { get; }
        //public IRepository<OrderItem> OrderItem { get; }


        public IRepository<KitchenOrder> KitchenOrder { get; }
        public IRepository<KitchenOrderStatusMessage> KitchenOrderStatusMessage { get; }

    }
}