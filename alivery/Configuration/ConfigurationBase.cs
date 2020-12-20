using System;
using System.ComponentModel;
using System.Linq;
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

        protected T ReadConfig<T>(string option, T defaultValue=default)
        {
            var result =db.GetById(option);
            if (result==null)
            {
                return defaultValue;
            }
            var value=TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(result.Value);
            return (T)value;

        }

        protected string ReadConfig(string option)
        {
            return ReadConfig<string>(option);
        }


        protected void WriteConfig<T>(string option,T value)
        {
            var result = db.Upsert(new Configuration
            {
                Id = option,
                Value = TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value)
            });
            if (result != 1)
                throw new Exception("Fatal:Unable to write config");
        }
    }
}