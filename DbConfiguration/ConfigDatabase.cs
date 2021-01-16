using DbConfiguration.Models;
using SqlBase;

namespace DbConfiguration
{
    public class ConfigDatabase: SqliteDatabase.SqliteDatabase
    {
        public ConfigDatabase(string pass) : this( pass, "config.db")
        {
        }

        public ConfigDatabase(string pass, string name) : base(name, pass)
        {
            Configuration = RegisterTable<ConfigurationModel>();

        }

        public IRepository<ConfigurationModel> Configuration { get; }

    }
}