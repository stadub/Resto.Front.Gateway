using System;
using System.ComponentModel;
using System.Threading.Tasks;
using DbConfiguration.Models;
using SqliteDatabase;

namespace DbConfiguration
{
    public class ConfigurationBase
    {
        private readonly IRepository<ConfigurationModel> db;
        public string ConfigType { get; }

        public ConfigurationBase(IRepository<ConfigurationModel> db, string configType)
        {
            this.db = db;
            ConfigType = configType;
        }

        protected async Task<T> ReadConfigAsync<T>(string option, T defaultValue=default)
        {
            var result = await db.GetByIdAsync(ConfigType + option);
            if (result==null)
            {
                return defaultValue;
            }
            var value=TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(result.Value);
            return (T)value;

        }

        protected async Task<string> ReadConfigAsync(string option)
        {
            return await ReadConfigAsync<string>(option);
        }

        protected T ReadConfig<T>(string option, T defaultValue = default)
        {
            return ReadConfigAsync<T>(option).Result;
        }

        protected string ReadConfig(string option)
        {
            return ReadConfigAsync<string>(option).Result;
        }

        protected void WriteConfig<T>(string option, T value)
        {
            WriteConfigAsync<T>(option, value).Wait();
        }


        protected async Task WriteConfigAsync<T>(string option,T value)
        {
            var result = await db.UpsertAsync(new ConfigurationModel
            {
                Id = ConfigType + option,
                Value = TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value)
            });
            if (result != 1)
                throw new Exception("Fatal:Unable to write config");
        }

        public string this[string option]
        {
            get => ReadConfig(option);
            set => WriteConfig(option, value);
        }
    }
}