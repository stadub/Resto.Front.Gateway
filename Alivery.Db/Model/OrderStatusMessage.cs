using SQLite;

namespace Alivery.Db.Model
{
 

    public class OrderStatusMessage : MessageStatusBase
    {
        [Indexed]
        public string OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        [Indexed]
        public int Revision { get; set; }


        [Indexed]
        public string IikoOrderId { get; set; }

    }
}