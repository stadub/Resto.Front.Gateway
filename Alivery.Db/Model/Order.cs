using System;
using SQLite;
using SqliteDatabase;

namespace Alivery.Db.Model
{
    public class Order: ValueObject
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


    }
}