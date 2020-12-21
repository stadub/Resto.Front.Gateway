using SQLite;

namespace alivery
{
    public class OrderStatusMessage : ValueObject
    {
        [Indexed]
        public string OrderId { get; set; }
        public int OrderStatus { get; set; }

        [Indexed]
        public int Revision { get; set; }

        public int Status { get; set; }

        [Indexed]
        public string OrderModelId { get; set; }
    }
}