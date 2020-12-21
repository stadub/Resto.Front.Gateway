using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Resto.Front.Api;

namespace alivery
{
    class Configurations
    {
        public Configurations(IRepository<Configuration> db)
        {
            Application = new AppConfiguration(db);
            KitchenOrderMessageQueue = new MessageQueueConfiguration(db, "KitchenOrder");
            OrderMessageQueue = new MessageQueueConfiguration(db, "Order");
        }

        public AppConfiguration Application { get; }
        public MessageQueueConfiguration OrderMessageQueue { get; }
        public MessageQueueConfiguration KitchenOrderMessageQueue { get; }

        public void OnFirstRun()
        {
            if (Application.FirstRun)
            {
                Application.SelfId = Guid.NewGuid().ToString();
                Application.FirstRun = false;
            }
        }

        private void LoadFromConfig(ConfigurationBase config, string name)
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            var configurationElement = appConfig.AppSettings.Settings[name];
            if (configurationElement != null)
            {
                var value = configurationElement.Value;
                PluginContext.Log.Info($"Config section {name} for config {config.ConfigType} reloaded from file");

                config[name] = value;
            }
        }

        public void LoadfromConfigFile()
        {


            LoadFromConfig(OrderMessageQueue, "OrderHostName");
            LoadFromConfig(OrderMessageQueue, "OrderUserName");
            LoadFromConfig(OrderMessageQueue, "OrderPassword");
            LoadFromConfig(OrderMessageQueue, "OrderVirtualHost");


            LoadFromConfig(KitchenOrderMessageQueue, "KitchenOrderHostName");
            LoadFromConfig(KitchenOrderMessageQueue, "KitchenOrderUserName");
            LoadFromConfig(KitchenOrderMessageQueue, "KitchenOrderPassword");
            LoadFromConfig(KitchenOrderMessageQueue, "KitchenOrderVirtualHost");

           
        }
    }
}
