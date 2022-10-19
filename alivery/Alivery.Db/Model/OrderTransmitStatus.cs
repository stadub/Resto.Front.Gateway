// Decompiled with JetBrains decompiler
// Type: Alivery.Db.Model.OrderTransmitStatus
// Assembly: Alivery.Db, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9F42CC1A-62E5-44F7-BF78-501FFB7A8F4A
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.Db.dll

using SqlBase;
using SQLite;
using System;

namespace Alivery.Db.Model
{
  public class OrderTransmitStatus : IValueObject
  {
    [PrimaryKey]
    public string Id { get; set; }

    [Indexed]
    public string OrderId { get; set; }

    public TransmitStatus TransmitStatus { get; set; }

    public DateTime Created { get; set; }

    public int IsObsolete { get; set; }

    [Indexed]
    public string OrderStatusMsgId { get; set; }
  }
}
