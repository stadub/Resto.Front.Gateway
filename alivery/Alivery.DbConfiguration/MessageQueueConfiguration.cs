// Decompiled with JetBrains decompiler
// Type: alivery.MessageQueueConfiguration
// Assembly: Alivery.DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED67DD70-179A-4D8F-B774-F5BB67601FD9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.DbConfiguration.dll

using Utils;

namespace alivery
{
  public class MessageQueueConfiguration : ConfigurationBase
  {
    public string HostName
    {
      get => this.ReadConfig<string>(nameof (HostName), "localhost");
      set => this.WriteConfig<string>(nameof (HostName), value);
    }

    public string UserName
    {
      get => this.ReadConfig(nameof (UserName));
      set => this.WriteConfig<string>(nameof (UserName), value);
    }

    public string Password
    {
      get => this.ReadConfig(nameof (Password));
      set => this.WriteConfig<string>(nameof (Password), value);
    }

    public string QueueName
    {
      get => this.ReadConfig(nameof (QueueName));
      set => this.WriteConfig<string>(nameof (QueueName), value);
    }

    public int Port
    {
      get => this.ReadConfig<int>(nameof (Port));
      set => this.WriteConfig<int>(nameof (Port), value);
    }

    public MessageQueueConfiguration(string configType)
      : base(configType)
    {
    }
  }
}
