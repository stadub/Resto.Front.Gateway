// Decompiled with JetBrains decompiler
// Type: Utils.Extensions
// Assembly: Utils, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 690B5CE8-EDEC-4045-B6F6-94F1A17BFD60
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\Utils.dll

using System.Collections.Generic;
using System.Linq;

namespace Utils
{
  public static class Extensions
  {
    public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
    {
      first = list.Count > 0 ? list[0] : default (T);
      rest = (IList<T>) list.Skip<T>(1).ToList<T>();
    }

    public static void Deconstruct<T>(
      this IList<T> list,
      out T first,
      out T second,
      out IList<T> rest)
    {
      first = list.Count > 0 ? list[0] : default (T);
      second = list.Count > 1 ? list[1] : default (T);
      rest = (IList<T>) list.Skip<T>(2).ToList<T>();
    }
  }
}
