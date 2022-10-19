// Decompiled with JetBrains decompiler
// Type: alivery.AppConfiguration
// Assembly: Alivery.DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED67DD70-179A-4D8F-B774-F5BB67601FD9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.DbConfiguration.dll

using Utils;

namespace alivery
{
  public class AppConfiguration : ConfigurationBase
  {
    public AppConfiguration()
      : base("app")
    {
    }

    public string SelfId
    {
      get => this.ReadConfig(nameof (SelfId));
      set => this.WriteConfig<string>(nameof (SelfId), value);
    }

    public bool FirstRun
    {
      get => this.ReadConfig<bool>(nameof (FirstRun), true);
      set => this.WriteConfig<bool>(nameof (FirstRun), value);
    }

    public string MsgServicePath
    {
      get => this.ReadConfig(nameof (MsgServicePath));
      set => this.WriteConfig<string>(nameof (MsgServicePath), value);
    }

    public string OrderDbPath
    {
      get => this.ReadConfig(nameof (OrderDbPath));
      set => this.WriteConfig<string>(nameof (OrderDbPath), value);
    }
  }
}
