// Decompiled with JetBrains decompiler
// Type: alivery.AliveryPlugin
// Assembly: alivery, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6907EBF8-7405-4C21-8554-36552C629C25
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\alivery.dll

using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using System;

namespace alivery
{
  [PluginLicenseModuleId(21005108)]
  public sealed class AliveryPlugin : IFrontPlugin, IDisposable
  {
    private Application app;

    public AliveryPlugin()
    {
      this.app = new Application();
      this.app.Start();
    }

    public void Dispose() => this.app.Dispose();
  }
}
