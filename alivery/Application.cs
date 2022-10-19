// Decompiled with JetBrains decompiler
// Type: alivery.Application
// Assembly: alivery, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6907EBF8-7405-4C21-8554-36552C629C25
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\alivery.dll

using Alivery.Db;
using Alivery.Db.Model;
using Newtonsoft.Json;
using Resto.Front.Api;
using Resto.Front.Api.Data.Common;
using Resto.Front.Api.Data.Kitchen;
using Resto.Front.Api.Data.Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace alivery
{
  public class Application : IDisposable
  {
    private ConfigDatabase configDb;
    private ConfigRegistry config;
    private readonly CompositeDisposable resources = new CompositeDisposable();
    private bool disposed;

    public Application()
    {
      this.configDb = new ConfigDatabase("appService.cfg", "суперсекретный пароль1");
      this.configDb.OpenAsync().Wait();
      this.config = new ConfigRegistry(this.configDb.Configuration);
      this.config.SyncFromConfigFile(Assembly.GetExecutingAssembly().Location);
      this.resources.Add(Disposable.Create(new Action(this.Dispose)));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Application Start()
    {
      Thread thread = new Thread((ThreadStart) (() => this.EntryPoint().Wait()));
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      while (!this.disposed)
      {
        try
        {
          this.StartMessageService();
        }
        catch (Exception ex)
        {
          PluginContext.Log.Error(ex.ToString(), ex);
        }
        Thread.Sleep(1000);
      }
      PluginContext.Log.Info("CookingPriorityManager started");
      return this;
    }

    private void StartMessageService()
    {
      Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      Process process = new Process();
      string msgServicePath = this.config.Application.MsgServicePath;
      process.StartInfo.FileName = msgServicePath;
      process.StartInfo.Arguments = "-n";
      process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.OutputDataReceived += (DataReceivedEventHandler) ((sender, args) => PluginContext.Log.Info(args.Data));
      process.ErrorDataReceived += (DataReceivedEventHandler) ((sender, args) => PluginContext.Log.Error(args.Data));
      process.Start();
      process.WaitForExit();
    }

    private async Task EntryPoint()
    {
      PluginContext.Log.Info("Start init...");
      await this.UpdateOrderStatus();
      this.resources.Add(PluginContext.Notifications.OrderChanged.Subscribe<EntityChangedEventArgs<IOrder>>((Action<EntityChangedEventArgs<IOrder>>) (x =>
      {
        try
        {
          this.ReceiveOrderUpdate(x).Wait();
        }
        catch (Exception ex)
        {
          PluginContext.Log.Error(ex.ToString(), ex);
        }
      })));
      this.resources.Add(PluginContext.Notifications.KitchenOrderChanged.Subscribe<EntityChangedEventArgs<IKitchenOrder>>((Action<EntityChangedEventArgs<IKitchenOrder>>) (x =>
      {
        try
        {
          this.ReceiveKitchenOrderUpdate(x).Wait();
        }
        catch (Exception ex)
        {
          PluginContext.Log.Error(ex.ToString(), ex);
        }
      })));
      while (true)
      {
        await Task.Delay(1000);
        if (!this.disposed)
        {
          try
          {
            await this.UpdateOrderStatus();
            await this.UpdateKOrderStatus();
          }
          catch (Exception ex)
          {
            PluginContext.Log.Error(ex.ToString(), ex);
          }
        }
        else
          break;
      }
      PluginContext.Log.Info("Exit...");
    }

    public async Task UpdateOrderStatus()
    {
      IReadOnlyList<IOrder> orders = PluginContext.Operations.GetOrders();
      OrderDatabase orderDb = new OrderDatabase(this.config.Application.OrderDbPath);
      IDisposable conn = await orderDb.OpenAsync();
      try
      {
        foreach (IOrder order1 in (IEnumerable<IOrder>) orders)
        {
          IOrder order = order1;
          string oderId = order.Id.ToString();
          List<Order> orderTransactions = await orderDb.Order.GetAllAsync((Expression<Func<Order, bool>>) (x => x.IikoOrderId == oderId));
          if (!orderTransactions.Any<Order>((Func<Order, bool>) (x => x.Revision == order.Revision)))
          {
            await this.StoreOrder(order, orderDb);
            orderTransactions = (List<Order>) null;
          }
        }
      }
      finally
      {
        conn?.Dispose();
      }
      orders = (IReadOnlyList<IOrder>) null;
      orderDb = (OrderDatabase) null;
      conn = (IDisposable) null;
    }

    public async Task UpdateKOrderStatus()
    {
      IReadOnlyList<IKitchenOrder> kOrders = PluginContext.Operations.GetKitchenOrders();
      OrderDatabase orderDb = new OrderDatabase(this.config.Application.OrderDbPath);
      IDisposable conn = await orderDb.OpenAsync();
      try
      {
        foreach (IKitchenOrder korder in (IEnumerable<IKitchenOrder>) kOrders)
        {
          string oderId = korder.Id.ToString();
          await this.StoreKitchenOrder(korder, orderDb);
          oderId = (string) null;
        }
      }
      finally
      {
        conn?.Dispose();
      }
      kOrders = (IReadOnlyList<IKitchenOrder>) null;
      orderDb = (OrderDatabase) null;
      conn = (IDisposable) null;
    }

    internal async Task ReceiveOrderUpdate(EntityChangedEventArgs<IOrder> statusUpdate)
    {
      IOrder order = statusUpdate.Entity;
      EntityEventType eventType = statusUpdate.EventType;
      switch (eventType)
      {
        default:
          OrderDatabase orderDb = new OrderDatabase(this.config.Application.OrderDbPath);
          IDisposable conn = await orderDb.OpenAsync();
          try
          {
            await this.StoreOrder(order, orderDb);
          }
          finally
          {
            conn?.Dispose();
          }
          order = (IOrder) null;
          orderDb = (OrderDatabase) null;
          conn = (IDisposable) null;
      }
    }

    private async Task ReceiveKitchenOrderUpdate(
      EntityChangedEventArgs<IKitchenOrder> statusUpdate)
    {
      IKitchenOrder order = statusUpdate.Entity;
      EntityEventType eventType = statusUpdate.EventType;
      switch (eventType)
      {
        default:
          OrderDatabase orderDb = new OrderDatabase(this.config.Application.OrderDbPath);
          IDisposable conn = await orderDb.OpenAsync();
          try
          {
            await this.StoreKitchenOrder(order, orderDb);
          }
          finally
          {
            conn?.Dispose();
          }
          order = (IKitchenOrder) null;
          orderDb = (OrderDatabase) null;
          conn = (IDisposable) null;
      }
    }

    private async Task StoreKitchenOrder(IKitchenOrder order, OrderDatabase orderDb)
    {
      string oderId = order.Id.ToString();
      string jsonString = JsonConvert.SerializeObject((object) order);
      KitchenOrder orderModel = new KitchenOrder()
      {
        CookingPriority = order.CookingPriority,
        Number = order.Number,
        BaseOrderId = order.BaseOrderId.ToString(),
        Json = jsonString,
        IikoOrderId = oderId
      };
      KitchenOrder kitchenOrder = await orderDb.KitchenOrder.AddAsync(orderModel);
      KitchenOrderTransmitStatus orderTransmitStatus = await orderDb.KitchenOrderTransmitStatus.AddAsync(new KitchenOrderTransmitStatus()
      {
        Created = DateTime.Now,
        TransmitStatus = TransmitStatus.Received,
        KitchenOrderId = orderModel.Id
      });
      oderId = (string) null;
      jsonString = (string) null;
      orderModel = (KitchenOrder) null;
    }

    private async Task StoreOrder(IOrder order, OrderDatabase orderDb)
    {
      string oderId = order.Id.ToString();
      string jsonString = JsonConvert.SerializeObject((object) order);
      Order orderModel = new Order()
      {
        Revision = order.Revision,
        CloseTime = order.CloseTime,
        OpenTime = order.OpenTime,
        IikoOrderId = oderId,
        OrderStatus = (Alivery.Db.Model.OrderStatus) order.Status,
        Json = jsonString
      };
      Order order1 = await orderDb.Order.AddAsync(orderModel);
      OrderTransmitStatus orderTransmitStatus = await orderDb.OrderTransmitStatus.AddAsync(new OrderTransmitStatus()
      {
        Created = DateTime.Now,
        TransmitStatus = TransmitStatus.Received,
        OrderId = orderModel.Id
      });
      oderId = (string) null;
      jsonString = (string) null;
      orderModel = (Order) null;
    }

    public void Dispose()
    {
      if (this.disposed)
        return;
      this.disposed = true;
    }
  }
}
