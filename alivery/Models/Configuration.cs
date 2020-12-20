using SQLite;

namespace alivery
{
    public class Configuration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}