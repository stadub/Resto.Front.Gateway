// Decompiled with JetBrains decompiler
// Type: Utils.Models.ConfigurationModel
// Assembly: DbConfiguration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B00EA45-AA52-4598-9063-5F141D43E5C2
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\DbConfiguration.dll

using SqlBase;
using SQLite;

namespace Utils.Models
{
  public class ConfigurationModel : IValueObject
  {
    [PrimaryKey]
    public string Id { get; set; }

    public string Value { get; set; }
  }
}
