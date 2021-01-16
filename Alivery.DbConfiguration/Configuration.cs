using System;
using DbConfiguration;
using DbConfiguration.Models;
using SqlBase;

namespace alivery
{
    public class ConfigRegistry: AppConfigurationBase
    {
        private readonly ConfigDatabase database;
        private bool disposed;

        public ConfigRegistry(string pass, string name) : base(null)
        {
            this.database =  new ConfigDatabase(pass, name);
            repository = database.Configuration;
            RegisterConfigSections();
        }

        public ConfigRegistry(ConfigDatabase db) : this(db.Configuration)
        {
            this.database = db;
            repository = database.Configuration;
        }


        public ConfigRegistry(IRepository<ConfigurationModel> repository) : base(repository)
        {
            //SyncFromConfigFile();
            RegisterConfigSections();
        }

        private void RegisterConfigSections()
        {

            OrderMessageQueue = base.RegisterConfigSection<MessageQueueConfiguration>("Order");

            KitchenOrderMessageQueue = base.RegisterConfigSection<MessageQueueConfiguration>("KitchenOrder");
            Application = base.RegisterConfigSection<AppConfiguration>();
        }

        public MessageQueueConfiguration OrderMessageQueue { get; private set; }
        public MessageQueueConfiguration KitchenOrderMessageQueue { get; private set; }
        public AppConfiguration Application { get; private set; }

        public void OnFirstRun()
        {
            if (Application.FirstRun)
            {
                Application.SelfId = Guid.NewGuid().ToString();
                Application.FirstRun = false;
            }
        }

        

        protected override void ReleaseUnmanagedResources()
        {
            if (disposed) return;
            database?.Close();
            disposed = true;
        }
    }
}
