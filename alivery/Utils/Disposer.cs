// Decompiled with JetBrains decompiler
// Type: Utils.Disposer
// Assembly: Utils, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 690B5CE8-EDEC-4045-B6F6-94F1A17BFD60
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Utils.dll

using System;
using System.Reflection;

namespace Utils
{
  public class Disposer : IDisposable
  {
    private readonly Action managedDispose;
    private readonly Action unmanagedDispose;
    private bool _disposed;

    public Disposer(IDisposable disposable)
    {
      MethodInfo destructor = disposable.GetType().GetMethod("Finalize", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
      this.managedDispose = new Action(disposable.Dispose);
      if (!(destructor != (MethodInfo) null))
        return;
      this.unmanagedDispose = (Action) (() => destructor.Invoke((object) disposable, new object[0]));
    }

    public static IDisposable Create(Action managedDispose, Action unmanagedDispose = null) => (IDisposable) new Disposer(managedDispose, unmanagedDispose);

    public Disposer(Action managedDispose, Action unmanagedDispose = null)
    {
      this.managedDispose = managedDispose;
      this.unmanagedDispose = unmanagedDispose;
    }

    private void ReleaseUnmanagedResources()
    {
      if (this.unmanagedDispose == null)
        return;
      this.unmanagedDispose();
    }

    private void ReleaseManagedResources()
    {
      if (this.managedDispose == null)
        return;
      this.managedDispose();
    }

    ~Disposer() => this.ReleaseUnmanagedResources();

    public void Dispose()
    {
      this.managedDispose();
      this.ReleaseUnmanagedResources();
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      if (disposing)
        this.ReleaseManagedResources();
      this.ReleaseUnmanagedResources();
      this._disposed = true;
    }
  }
}
