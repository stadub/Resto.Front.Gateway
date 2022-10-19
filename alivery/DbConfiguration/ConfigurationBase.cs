// Decompiled with JetBrains decompiler
// Type: Utils.ConfigurationBase
// Assembly: DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B00EA45-AA52-4598-9063-5F141D43E5C2
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\DbConfiguration.dll

using SqlBase;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Utils.Models;

namespace Utils
{
  public class ConfigurationBase
  {
    protected IRepository<ConfigurationModel> db;

    public string ConfigType { get; protected set; }

    public ConfigurationBase(string configType) => this.ConfigType = configType;

    public ConfigurationBase Initialize(IRepository<ConfigurationModel> db)
    {
      this.db = db;
      return this;
    }

    protected async Task<T> ReadConfigAsync<T>(string option, T defaultValue = null)
    {
      ConfigurationModel result = await this.db.GetByIdAsync(this.ConfigType + option);
      if (result == null)
        return defaultValue;
      object value = TypeDescriptor.GetConverter(typeof (T)).ConvertFromString(result.Value);
      return (T) value;
    }

    protected async Task<string> ReadConfigAsync(string option)
    {
      string str = await this.ReadConfigAsync<string>(option);
      return str;
    }

    protected T ReadConfig<T>(string option, T defaultValue = null) => this.ReadConfigAsync<T>(option).Result;

    protected string ReadConfig(string option) => this.ReadConfigAsync<string>(option).Result;

    protected void WriteConfig<T>(string option, T value) => this.WriteConfigAsync<T>(option, value).Wait();

    protected async Task WriteConfigAsync<T>(string option, T value)
    {
      ConfigurationModel result = await this.db.UpsertAsync(new ConfigurationModel()
      {
        Id = this.ConfigType + option,
        Value = TypeDescriptor.GetConverter(typeof (T)).ConvertToString((object) (T) value)
      });
      result = result != null ? (ConfigurationModel) null : throw new Exception("Fatal:Unable to write config");
    }

    public string this[string option]
    {
      get => this.ReadConfig(option);
      set => this.WriteConfig<string>(option, value);
    }
  }
}
