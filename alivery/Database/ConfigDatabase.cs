namespace alivery
{
    public class ConfigDatabase: DatabaseBase
    {
        public ConfigDatabase() : base("config", "суперсекретный пароль")
        {
            Configuration = RegisterTable<Configuration>();

        }

        public IRepository<Configuration> Configuration { get; }

    }
}