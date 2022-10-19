// Decompiled with JetBrains decompiler
// Type: alivery.ConfigRegistry
// Assembly: Alivery.DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED67DD70-179A-4D8F-B774-F5BB67601FD9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.DbConfiguration.dll

using SqlBase;
using System;
using Utils;
using Utils.Models;

namespace alivery
{
  public class ConfigRegistry : AppConfigurationBase
  {
    private readonly ConfigDatabase database;
    private bool disposed;

    public ConfigRegistry(string pass, string name)
      : base((IRepository<ConfigurationModel>) null)
    {
      this.database = new ConfigDatabase(pass, name);
      this.repository = this.database.Configuration;
      this.RegisterConfigSections();
    }

    public ConfigRegistry(ConfigDatabase db)
      : this(db.Configuration)
    {
      this.database = db;
      this.repository = this.database.Configuration;
    }

    public ConfigRegistry(IRepository<ConfigurationModel> repository)
      : base(repository)
    {
      this.RegisterConfigSections();
    }

    private void RegisterConfigSections()
    {
      this.OrderMessageQueue = this.RegisterConfigSection<MessageQueueConfiguration>("Order");
      this.KitchenOrderMessageQueue = this.RegisterConfigSection<MessageQueueConfiguration>("KitchenOrder");
      this.Application = this.RegisterConfigSection<AppConfiguration>();
    }

    public MessageQueueConfiguration OrderMessageQueue { get; private set; }

    public MessageQueueConfiguration KitchenOrderMessageQueue { get; private set; }

    public AppConfiguration Application { get; private set; }

    public void OnFirstRun()
    {
      if (!this.Application.FirstRun)
        return;
      this.Application.SelfId = Guid.NewGuid().ToString();
      this.Application.FirstRun = false;
    }

    protected override void ReleaseUnmanagedResources()
    {
      if (this.disposed)
        return;
      this.database?.Close();
      this.disposed = true;
    }
  }
}
