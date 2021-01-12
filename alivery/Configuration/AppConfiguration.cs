using DbConfiguration.Models;
using SQLite;
using SqliteDatabase;

namespace alivery
{
    public class AppConfiguration : ConfigurationBase
    {
        public AppConfiguration(IRepository<ConfigurationModel> db) : base(db, "app")
        {
        }

        public string SelfId
        {
            get => ReadConfig("SelfId");
            set => WriteConfig("SelfId", value);
        }


        public bool FirstRun
        {
            get => ReadConfig<bool>("FirstRun", true);
            set => WriteConfig("FirstRun", value);
        }
    }
}