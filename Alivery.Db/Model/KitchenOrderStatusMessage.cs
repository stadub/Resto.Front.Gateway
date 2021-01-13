﻿using SQLite;

namespace Alivery.Db.Model
{
    public class KitchenOrderStatusMessage : MessageStatusBase
    {
        [Indexed]
        public string OrderId { get; set; }


        public int OrderStatus { get; set; }

        [Indexed]
        public int Revision { get; set; }

        [Indexed]
        public string OrderModelId { get; set; }


        [Indexed]
        public string IikoOrderId { get; set; }
    }
}