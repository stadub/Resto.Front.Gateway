using SQLite;

namespace alivery
{
    public class OrderStatusMessage : ValueObject
    {
        public string OrderId { get; set; }
        public int OrderStatus { get; set; }
        public int Revision { get; set; }

    }
}