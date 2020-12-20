using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace alivery.db
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string JsonData { get; set; }
    }

}
