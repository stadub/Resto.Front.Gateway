using System;
using SQLite;

namespace alivery
{
    public class OrderItem : ValueObject
    {
        public int TransactionNumber { get; set; }

        public string OrderId { get; set; }

        public DateTime CookingStartTime { get; set; }
        public DateTime CookingFinishTime { get; set; }

        public string KitchenId { get; set; }
        public string GuestId { get; set; }


        public int Status { get; set; }

        public string Json { get; set; }
        public string ProductId { get; set; }
    }
}