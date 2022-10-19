// Decompiled with JetBrains decompiler
// Type: Utils.ConfigDatabase
// Assembly: DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B00EA45-AA52-4598-9063-5F141D43E5C2
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\DbConfiguration.dll

using SqlBase;
using System.Runtime.InteropServices;
using Utils.Models;

namespace Utils
{
  [Guid("237c89ab-d689-41c6-b745-10d0926ead99")]
  public class ConfigDatabase : SqliteDatabase.SqliteDatabase
  {
    public ConfigDatabase(string pass)
      : this("config.db", pass)
    {
    }

    public ConfigDatabase(string name, string pass)
      : base(name, pass)
    {
      this.Configuration = this.RegisterTable<ConfigurationModel>();
    }

    public IRepository<ConfigurationModel> Configuration { get; }
  }
}
