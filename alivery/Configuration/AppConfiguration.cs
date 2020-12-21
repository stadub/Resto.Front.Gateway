using SQLite;

namespace alivery
{
    public class AppConfiguration : ConfigurationBase
    {
        public AppConfiguration(IRepository<Configuration> db) : base(db, "app")
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