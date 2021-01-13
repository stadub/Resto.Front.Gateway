using System;
using SqlBase;
using SQLite;

namespace Alivery.Db.Model
{
    public enum OrderStatus
    {
        /// <summary>Newly created order. This status corresponds to lifetime from creation to ready-to-pay state.</summary>
        New,
        /// <summary>Bill cheque printed order. This status corresponds to lifetime between billing and payment.</summary>
        Bill,
        /// <summary>Paid order.</summary>
        Closed,
        /// <summary>Deleted order.</summary>
        Deleted,
    }

    public class Order : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Json { get; set; }

        public OrderStatus OrderStatus { get; set; }

        [Indexed]
        public int Revision { get; set; }

        public DateTime OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        [Indexed]
        public string IikoOrderId { get; set; }

    }

}
