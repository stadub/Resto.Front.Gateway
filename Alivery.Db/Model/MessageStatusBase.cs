using SqlBase;
using SQLite;

namespace Alivery.Db.Model
{
    public class MessageStatusBase : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Json { get; set; }
    }
}