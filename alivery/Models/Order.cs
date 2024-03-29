﻿using System;
using Resto.Front.Api.Data.Orders;
using SQLite;

namespace alivery
{
    public class Order: ValueObject
    {
        [Indexed]
        public string Json { get; set; }
        public OrderStatus Status { get; set; }

        [Indexed]
        public int Revision { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }

        [Indexed]
        public string OrderId { get; set; }


    }
}