using SQLite;

namespace Alivery.Db.Model
{
    public class KitchenOrderStatusMessage : MessageStatusBase
    {
        [Indexed]
        public string OrderId { get; set; }
        public int Number { get; set; }

        public int CookingPriority { get; set; }


        [Indexed]
        public string OrderModelId { get; set; }

        [Indexed]
        public string BaseOrderId { get; set; }
    }
}