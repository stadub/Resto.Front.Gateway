using DbConfiguration.Models;
using SqliteDatabase;

namespace DbConfiguration
{
    public class ConfigDatabase: DatabaseBase
    {
        public ConfigDatabase(string pass) : base("config", pass)
        {
            Configuration = RegisterTable<ConfigurationModel>();

        }

        public IRepository<ConfigurationModel> Configuration { get; }

    }
}