using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace alivery
{
    public class ConfigurationBase
    {
        private readonly IRepository<Configuration> db;

        public ConfigurationBase(IRepository<Configuration> db)
        {
            this.db = db;
        }

        protected async Task<T> ReadConfigAsync<T>(string option, T defaultValue=default)
        {
            var result = await db.GetByIdAsync(option);
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
            var result = await db.UpsertAsync(new Configuration
            {
                Id = option,
                Value = TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value)
            });
            if (result != 1)
                throw new Exception("Fatal:Unable to write config");
        }
    }
}