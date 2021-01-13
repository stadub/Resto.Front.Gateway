
using SqlBase;
using SQLite;

namespace DbConfiguration.Models
{
    public class ConfigurationModel : IValueObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Value { get; set; }
    }
}