using System;
using DbConfiguration;
using DbConfiguration.Models;
using SqlBase;

namespace alivery
{
    public class ConfigRegistry: AppConfigurationBase
    {
        public ConfigRegistry(IRepository<ConfigurationModel> db) : base(db)
        {

            OrderMessageQueue= base.RegisterConfigSection( ()=>new MessageQueueConfiguration(db, "Order"));
            KitchenOrderMessageQueue = base.RegisterConfigSection( ()=>new MessageQueueConfiguration(db, "KitchenOrder"));
            Application = base.RegisterConfigSection( ()=>new AppConfiguration(db));

            //SyncFromConfigFile();
        }

        public MessageQueueConfiguration OrderMessageQueue { get; }
        public MessageQueueConfiguration KitchenOrderMessageQueue { get; }
        public AppConfiguration Application { get; }

        public void OnFirstRun()
        {
            if (Application.FirstRun)
            {
                Application.SelfId = Guid.NewGuid().ToString();
                Application.FirstRun = false;
            }
        }

    }
}
