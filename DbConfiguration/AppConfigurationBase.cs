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
    public class AppConfigurationBase:IDisposable
    {
        protected IRepository<ConfigurationModel> repository;
        private bool disposed;
        protected Dictionary<string, ConfigurationBase> ConfigurationSections { get; }
        public AppConfigurationBase(IRepository<ConfigurationModel> repository)
        {
            this.repository = repository;
            ConfigurationSections = new Dictionary<string, ConfigurationBase>();
        }

        protected T RegisterConfigSection<T>(Func<T> initFunc) where  T: ConfigurationBase
        {
            var configSection = initFunc();
            return RegisterConfig(configSection);
        }

        protected T RegisterConfigSection<T>() where T : ConfigurationBase, new()
        {
            var configSection = new T();
            return RegisterConfig(configSection);
        }

        protected T RegisterConfigSection<T>(string configName) where T : ConfigurationBase
        {
            var configSection =  (T)Activator.CreateInstance(typeof(T), configName);
            return RegisterConfig(configSection);
        }

        private T RegisterConfig<T>(T configSection) where T : ConfigurationBase
        {
            ConfigurationSections.Add(configSection.ConfigType, configSection);
            configSection.Initialize(repository);
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
                    var value = key.Attributes["value"].Value;

                    configuration[configName] = value;
                    //PluginContext.Log.Warn($"App config section [{configSection}] is not match to any db config section");
                }
            }

        }


        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            ReleaseUnmanagedResources();
            if (disposing)
            {
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AppConfigurationBase()
        {
            Dispose(false);
        }
    }
}