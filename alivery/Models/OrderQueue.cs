using SQLite;

namespace alivery
{
    public class OrderQueue
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Json { get; set; }
    }
}