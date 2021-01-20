using SQLite;

namespace Alivery.Db.Model
{
    public class KitchenOrderStatusMessage : MessageStatusBase
    {
        [Indexed]
        public string OrderId { get; set; }

        [Indexed]
        public string OrderModelId { get; set; }


        [Indexed]
        public string IikoOrderId { get; set; }
    }
}