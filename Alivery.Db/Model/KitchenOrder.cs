using System;
using SQLite;
using SqliteDatabase;

namespace Alivery.Db.Model
{
    public class KitchenOrder : ValueObject
    {
        [Indexed]
        public string Json { get; set; }
        public int Status { get; set; }

        [Indexed]
        public int Revision { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }

        [Indexed]
        public string OrderId { get; set; }


        public int Number { get; set; }

        public int CookingPriority { get; set; }



        [Indexed]
        public string BaseOrderId { get; set; }

    }
}