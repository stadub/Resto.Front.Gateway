namespace alivery
{
    public class ConfigDatabase: DatabaseBase
    {
        public ConfigDatabase(string pass) : base("config", pass)
        {
            Configuration = RegisterTable<Configuration>();

        }

        public IRepository<Configuration> Configuration { get; }

    }
}