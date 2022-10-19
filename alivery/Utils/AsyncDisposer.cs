// Decompiled with JetBrains decompiler
// Type: Utils.AsyncDisposer
// Assembly: Utils, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 690B5CE8-EDEC-4045-B6F6-94F1A17BFD60
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Utils.dll

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Utils
{
  public class AsyncDisposer : IAsyncDisposable, IDisposable
  {
    private readonly Task managedDispose;
    private readonly Action unmanagedDispose;
    private bool _disposed;

    public AsyncDisposer(IAsyncDisposable disposable)
    {
      MethodInfo destructor = disposable.GetType().GetMethod("Finalize", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
      this.managedDispose = new Task((Action) (async () => await disposable.DisposeAsync()));
      if (!(destructor != (MethodInfo) null))
        return;
      this.unmanagedDispose = (Action) (() => destructor.Invoke((object) disposable, new object[0]));
    }

    public static IAsyncDisposable Create(
      Task managedDispose,
      Action unmanagedDispose = null)
    {
      return (IAsyncDisposable) new AsyncDisposer(managedDispose, unmanagedDispose);
    }

    public AsyncDisposer(Task managedDispose, Action unmanagedDispose = null)
    {
      this.managedDispose = managedDispose;
      this.unmanagedDispose = unmanagedDispose;
    }

    private void ReleaseManagedResources() => this.ReleaseManagedResourcesAsync().Wait();

    private void ReleaseUnmanagedResources()
    {
      if (this._disposed || this.unmanagedDispose == null)
        return;
      this.unmanagedDispose();
    }

    private async Task ReleaseManagedResourcesAsync()
    {
      if (this._disposed || this.managedDispose == null)
        return;
      await this.managedDispose;
    }

    ~AsyncDisposer() => this.ReleaseUnmanagedResources();

    public void Dispose()
    {
      if (this._disposed)
        return;
      this.ReleaseManagedResources();
      this.ReleaseUnmanagedResources();
      GC.SuppressFinalize((object) this);
      this._disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
      await this.DisposeAsyncCore();
      GC.SuppressFinalize((object) this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
      if (this._disposed)
        return;
      await this.ReleaseManagedResourcesAsync();
      this.ReleaseUnmanagedResources();
      this._disposed = true;
    }
  }
}
