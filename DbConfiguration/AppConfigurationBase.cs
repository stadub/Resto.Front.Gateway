using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Xml;
using DbConfiguration.Models;
using SqlBase;

namespace DbConfiguration
{
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


        public void SyncFromConfigFile(string file)
        {

            var xmlDocument = new XmlDocument();
            xmlDocument.Load($"{file}.config");

            var section = xmlDocument.SelectSingleNode("//configuration/ConfigSettings");


            foreach (XmlNode key in section.ChildNodes)
            {

                var (configSection, configName, _) = key.Attributes["key"].Value.Split(':');

                if (ConfigurationSections.TryGetValue(configSection, out var configuration))
                {
                    var value = key.Attributes["value"].Value;

                    configuration[configName] = value;
                    //PluginContext.Log.Info($"Config section [{configSection}:{configName}] reloaded from file");

                }
                else
                {
                    //PluginContext.Log.Warn($"App config section [{configSection}] is not match to any db config section");
                }
            }

        }

    }
}