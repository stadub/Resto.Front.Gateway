using SQLite;

namespace alivery
{
    public class OrderStatusMessage : MessageStatusBase
    {
        [Indexed]
        public string OrderId { get; set; }
        public int OrderStatus { get; set; }

        [Indexed]
        public int Revision { get; set; }


        [Indexed]
        public string OrderModelId { get; set; }
    }
}