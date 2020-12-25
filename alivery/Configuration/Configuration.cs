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

            var configurationElement = appConfig.AppSettings.Settings[config.ConfigType + name];
            if (configurationElement != null)
            {
                var value = configurationElement.Value;
                PluginContext.Log.Info($"Config section [{config.ConfigType}:{name}] reloaded from file");

                config[name] = value;
            }
        }

        public void LoadfromConfigFile()
        {


            LoadFromConfig(OrderMessageQueue, "HostName");
            LoadFromConfig(OrderMessageQueue, "UserName");
            LoadFromConfig(OrderMessageQueue, "Password");
            LoadFromConfig(OrderMessageQueue, "VirtualHost");
            LoadFromConfig(OrderMessageQueue, "QueueName");
            

            LoadFromConfig(KitchenOrderMessageQueue, "HostName");
            LoadFromConfig(KitchenOrderMessageQueue, "UserName");
            LoadFromConfig(KitchenOrderMessageQueue, "Password");
            LoadFromConfig(KitchenOrderMessageQueue, "VirtualHost");
            LoadFromConfig(KitchenOrderMessageQueue, "QueueName");

           
        }
    }
}
