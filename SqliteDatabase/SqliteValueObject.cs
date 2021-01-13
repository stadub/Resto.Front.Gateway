using SqlBase;
using SQLite;

namespace SqliteDatabase
{
    public class SqliteValueObject: IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}