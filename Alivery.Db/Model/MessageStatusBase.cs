using SqliteDatabase;

namespace Alivery.Db.Model
{
    public class MessageStatusBase: ValueObject
    {
        public int Status { get; set; }
    }
}