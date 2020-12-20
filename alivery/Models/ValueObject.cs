using SQLite;

namespace alivery
{
    public class ValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }
    }
}