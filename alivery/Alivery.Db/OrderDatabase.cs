// Decompiled with JetBrains decompiler
// Type: Alivery.Db.OrderDatabase
// Assembly: Alivery.Db, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9F42CC1A-62E5-44F7-BF78-501FFB7A8F4A
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Alivery.Db.dll

using SqlBase;
using System.Runtime.InteropServices;

namespace Alivery.Db
{
  [Guid("237c89ab-d689-41c6-b745-10d0926ead99")]
  public class OrderDatabase : SqliteDatabase.SqliteDatabase
  {
    private bool isOpen;

    public OrderDatabase(string databasePath)
      : base(databasePath, (string) null)
    {
      this.Order = this.RegisterTable<Alivery.Db.Model.Order>();
      this.OrderTransmitStatus = this.RegisterTable<Alivery.Db.Model.OrderTransmitStatus>();
      this.OrderStatusMessage = this.RegisterTable<Alivery.Db.Model.OrderStatusMessage>();
      this.KitchenOrder = this.RegisterTable<Alivery.Db.Model.KitchenOrder>();
      this.KitchenOrderTransmitStatus = this.RegisterTable<Alivery.Db.Model.KitchenOrderTransmitStatus>();
      this.KitchenOrderStatusMessage = this.RegisterTable<Alivery.Db.Model.KitchenOrderStatusMessage>();
    }

    public IRepository<Alivery.Db.Model.Order> Order { get; }

    public IRepository<Alivery.Db.Model.OrderTransmitStatus> OrderTransmitStatus { get; }

    public IRepository<Alivery.Db.Model.OrderStatusMessage> OrderStatusMessage { get; }

    public IRepository<Alivery.Db.Model.KitchenOrder> KitchenOrder { get; }

    public IRepository<Alivery.Db.Model.KitchenOrderTransmitStatus> KitchenOrderTransmitStatus { get; }

    public IRepository<Alivery.Db.Model.KitchenOrderStatusMessage> KitchenOrderStatusMessage { get; }
  }
}
