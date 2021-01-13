using DbConfiguration.Models;
using SqlBase;

namespace DbConfiguration
{
    public class ConfigDatabase: SqliteDatabase.SqliteDatabase
    {
        public ConfigDatabase(string pass) : base("config", pass)
        {
            Configuration = RegisterTable<ConfigurationModel>();

        }

        public IRepository<ConfigurationModel> Configuration { get; }

    }
}