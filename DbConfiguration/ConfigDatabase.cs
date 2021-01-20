using System.Runtime.InteropServices;
using SqlBase;
using Utils.Models;

namespace Utils
{
    [Guid("237c89ab-d689-41c6-b745-10d0926ead99")]
    public class ConfigDatabase: SqliteDatabase.SqliteDatabase
    {
        public ConfigDatabase(string pass) : this( "config.db" ,pass)
        {
        }

        public ConfigDatabase(string name, string pass) : base(name, pass)
        {
            Configuration = RegisterTable<ConfigurationModel>();

        }

        public IRepository<ConfigurationModel> Configuration { get; }

    }
}