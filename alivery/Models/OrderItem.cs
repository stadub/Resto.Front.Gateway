using System;
using SQLite;
using SqliteDatabase;

namespace alivery
{
    public class OrderItem : ValueObject
    {
        public int TransactionNumber { get; set; }

        [Indexed]
        public string OrderId { get; set; }

        public DateTime CookingStartTime { get; set; }
        public DateTime CookingFinishTime { get; set; }

        [Indexed]
        public string KitchenId { get; set; }
        
        public string GuestId { get; set; }

        public int Status { get; set; }

        public string Json { get; set; }

        [Indexed]
        public string ProductId { get; set; }
    }
}