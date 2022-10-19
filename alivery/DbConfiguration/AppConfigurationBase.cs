// Decompiled with JetBrains decompiler
// Type: Utils.AppConfigurationBase
// Assembly: DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B00EA45-AA52-4598-9063-5F141D43E5C2
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\DbConfiguration.dll

using SqlBase;
using System;
using System.Collections.Generic;
using System.Xml;
using Utils.Models;

namespace Utils
{
  public class AppConfigurationBase : IDisposable
  {
    protected IRepository<ConfigurationModel> repository;
    private bool disposed;

    protected Dictionary<string, ConfigurationBase> ConfigurationSections { get; }

    public AppConfigurationBase(IRepository<ConfigurationModel> repository)
    {
      this.repository = repository;
      this.ConfigurationSections = new Dictionary<string, ConfigurationBase>();
    }

    protected T RegisterConfigSection<T>(Func<T> initFunc) where T : ConfigurationBase => this.RegisterConfig<T>(initFunc());

    protected T RegisterConfigSection<T>() where T : ConfigurationBase, new() => this.RegisterConfig<T>(new T());

    protected T RegisterConfigSection<T>(string configName) where T : ConfigurationBase => this.RegisterConfig<T>((T) Activator.CreateInstance(typeof (T), (object) configName));

    private T RegisterConfig<T>(T configSection) where T : ConfigurationBase
    {
      this.ConfigurationSections.Add(configSection.ConfigType, (ConfigurationBase) configSection);
      configSection.Initialize(this.repository);
      return configSection;
    }

    public void SyncFromConfigFile(string file)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(file + ".config");
      foreach (XmlNode childNode in xmlDocument.SelectSingleNode("//configuration/ConfigSettings").ChildNodes)
      {
        string first;
        string second;
        ((IList<string>) childNode.Attributes["key"].Value.Split(':')).Deconstruct<string>(out first, out second, out IList<string> _);
        string key = first;
        string option = second;
        ConfigurationBase configurationBase;
        if (this.ConfigurationSections.TryGetValue(key, out configurationBase))
        {
          string str = childNode.Attributes["value"].Value;
          configurationBase[option] = str;
        }
        else
        {
          string str = childNode.Attributes["value"].Value;
          configurationBase[option] = str;
        }
      }
    }

    protected virtual void ReleaseUnmanagedResources()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      this.ReleaseUnmanagedResources();
      if (!disposing)
        ;
      this.disposed = true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~AppConfigurationBase() => this.Dispose(false);
  }
}
