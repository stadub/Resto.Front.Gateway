// Decompiled with JetBrains decompiler
// Type: SqlBase.DatabaseBase`1
// Assembly: SqlBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BAA73A5F-063C-4FEF-9A26-1FCC002845B9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\SqlBase.dll

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlBase
{
  public abstract class DatabaseBase<TDatabase> : IDisposable, IAsyncDisposable
  {
    private bool disposed;

    protected TDatabase Connection { get; private set; }

    protected Dictionary<string, IDbProvider<TDatabase>> Tables { get; }

    public DatabaseBase(string name, string password) => this.Tables = new Dictionary<string, IDbProvider<TDatabase>>();

    public async Task<IDisposable> OpenAsync()
    {
      TDatabase database = await this.CreateConnectionAsync();
      this.Connection = database;
      database = default (TDatabase);
      foreach (KeyValuePair<string, IDbProvider<TDatabase>> table1 in this.Tables)
      {
        KeyValuePair<string, IDbProvider<TDatabase>> table = table1;
        await table.Value.InitAsync(this.Connection);
        table = new KeyValuePair<string, IDbProvider<TDatabase>>();
      }
      return (IDisposable) this;
    }

    protected abstract Task<TDatabase> CreateConnectionAsync();

    protected IRepository<TValue> RegisterTable<TValue>() where TValue : IValueObject, new()
    {
      IDbProvider<TDatabase> dbProvider = this.RegisterRepository<TValue>();
      this.Tables.Add(typeof (TValue).Name, dbProvider);
      return (IRepository<TValue>) dbProvider;
    }

    protected abstract IDbProvider<TDatabase> RegisterRepository<TValue>() where TValue : IValueObject, new();

    public void Close() => this.Dispose();

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      this.disposed = true;
    }

    protected virtual async Task DisposeConnectionAsync()
    {
      if (this.disposed)
        return;
      this.disposed = true;
    }

    public void Dispose()
    {
      if (this.disposed)
        return;
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
      this.disposed = true;
    }

    public async ValueTask DisposeAsync() => await this.DisposeConnectionAsync();
  }
}
