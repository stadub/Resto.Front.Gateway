using SQLite;

namespace SqliteDatabase
{
    public class ValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}