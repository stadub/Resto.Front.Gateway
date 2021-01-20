using System;
using SqlBase;
using SQLite;

namespace Alivery.Db.Model
{
    public class KitchenOrder : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public string Json { get; set; }

        public int Number { get; set; }

        public int CookingPriority { get; set; }



        [Indexed]
        public string BaseOrderId { get; set; }


        [Indexed]
        public string IikoOrderId { get; set; }

    }

    public class KitchenOrderTransmitStatus : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public string KitchenOrderId { get; set; }

        public TransmitStatus TransmitStatus { get; set; }

        public DateTime Created { get; set; }

        public int IsObsolete { get; set; }
        public string KitchenOrderStatusMsgId { get; set; }
    }
}