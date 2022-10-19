// Decompiled with JetBrains decompiler
// Type: Utils.Runtime
// Assembly: Utils, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 690B5CE8-EDEC-4045-B6F6-94F1A17BFD60
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Utils.dll

using System;
using System.Threading;

namespace Utils
{
  public class Runtime
  {
    public static void Exclusive(string appId, Action action)
    {
      bool createdNew;
      using (Mutex mutex = new Mutex(false, "Global\\{" + appId + "}", out createdNew))
      {
        bool flag = false;
        try
        {
          try
          {
            flag = mutex.WaitOne(10000, false);
            if (!flag)
              throw new TimeoutException("Timeout waiting for exclusive access");
          }
          catch (AbandonedMutexException ex)
          {
            flag = true;
          }
          action();
        }
        finally
        {
          if (flag)
            mutex.ReleaseMutex();
        }
      }
    }
  }
}
