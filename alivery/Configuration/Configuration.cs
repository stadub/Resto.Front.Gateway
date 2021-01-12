using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbConfiguration.Models;
using Resto.Front.Api;
using SqliteDatabase;

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

    public class AppConfigurationBase
    {
        protected Dictionary<string, ConfigurationBase> ConfigurationSections { get; }
        public AppConfigurationBase(IRepository<ConfigurationModel> db)
        {
            ConfigurationSections = new Dictionary<string, ConfigurationBase>();
        }

        protected T RegisterConfigSection<T>(Func<T> initFunc) where  T: ConfigurationBase
        {
            var configSection = initFunc();
            ConfigurationSections.Add(configSection.ConfigType, configSection);
            return configSection;
        }

        protected T RegisterConfigSection<T>() where T : ConfigurationBase, new()
        {
            var configSection = new T();
            ConfigurationSections.Add(configSection.ConfigType, configSection);
            return configSection;
        }


        public void SyncFromConfigFile()
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection("ConfigSettings");

            foreach (var key in section.AllKeys)
            {
                var split = key.Split(':');

                var (configSection, configName) = (split[0], split[1]);

                if (ConfigurationSections.TryGetValue(configSection, out var configuration))
                {
                    var value = section.Get(key);

                    configuration[configName] = value;
                    PluginContext.Log.Info($"Config section [{configSection}:{configName}] reloaded from file");

                }
                else
                {
                    PluginContext.Log.Warn($"App config section [{configSection}] is not match to any db config section");
                }
            }

        }

    }
}
