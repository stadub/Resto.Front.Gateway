namespace alivery
{
    public class OrderDatabase : DatabaseBase
    {
        public OrderDatabase() : base("alivery", "Сорок два!")
        {
            Order = RegisterTable<Order> ( );
            Product = RegisterTable<Product> ();
            OrderStatusMessage = RegisterTable<OrderStatusMessage>();
            OrderItem = RegisterTable<OrderItem>();
        }

        public IRepository<Order> Order { get; }
        public IRepository<Product> Product { get; }
        public IRepository<OrderStatusMessage> OrderStatusMessage { get; }
        public IRepository<OrderItem> OrderItem { get; }

    }
}