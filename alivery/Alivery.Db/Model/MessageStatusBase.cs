﻿// Decompiled with JetBrains decompiler
// Type: Alivery.Db.Model.MessageStatusBase
// Assembly: Alivery.Db, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9F42CC1A-62E5-44F7-BF78-501FFB7A8F4A
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.Db.dll

using SqlBase;
using SQLite;

namespace Alivery.Db.Model
{
  public class MessageStatusBase : IValueObject
  {
    [PrimaryKey]
    public string Id { get; set; }

    public string Json { get; set; }
  }
}
