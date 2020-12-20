namespace alivery
{
    public class AppConfiguration : ConfigurationBase
    {
        public AppConfiguration(Database db) : base(db)
        {
        }

        public string SelfId
        {
            get => ReadConfig("SelfId");
            set => WriteConfig("SelfId", value);
        }


        public bool FirstRun
        {
            get => ReadConfig<bool>("FirstRun");
            set => WriteConfig("FirstRun", value);
        }
    }
}