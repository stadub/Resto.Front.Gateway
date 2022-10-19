// Decompiled with JetBrains decompiler
// Type: SqlBase.IDbProvider`1
// Assembly: SqlBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BAA73A5F-063C-4FEF-9A26-1FCC002845B9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\SqlBase.dll

using System.Threading.Tasks;

namespace SqlBase
{
  public interface IDbProvider<TDatabase>
  {
    Task InitAsync(TDatabase database);
  }
}
