using System;
using System.ComponentModel;
using System.Linq;

namespace alivery
{
    public class ConfigurationBase
    {
        protected readonly Database db;

        public ConfigurationBase(Database db)
        {
            this.db = db;
        }

        protected T ReadConfig<T>(string option)
        {
            var result =this.db.Read<Configuration>(configuration => configuration.Name == option);
            var config= result.FirstOrDefault();
            if (config == null)
                return default(T);
            var value=TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(config.Value);
            return (T)value;

        }

        protected string ReadConfig(string option)
        {
            return ReadConfig<string>(option);
        }


        protected void WriteConfig<T>(string option,T value)
        {
            var result = this.db.Store<Configuration>(new Configuration
            {
                Name = option,
                Value = TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value)
            });
            if (result != 0)
                throw new Exception("Fatal:Unable to write config");
        }
    }
}