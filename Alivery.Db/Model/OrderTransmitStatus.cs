using System;
using SqlBase;
using SQLite;

namespace Alivery.Db.Model
{
    public class OrderTransmitStatus : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public string OrderId { get; set; }

        public TransmitStatus TransmitStatus { get; set; }

        public DateTime Created { get; set; }

        public int IsObsolete { get; set; }

        [Indexed]
        public string OrderStatusMsgId { get; set; }
    }
}