// Decompiled with JetBrains decompiler
// Type: SqlBase.IRepository`1
// Assembly: SqlBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BAA73A5F-063C-4FEF-9A26-1FCC002845B9
// Assembly location: C:\Cache\45d56e93\Downloads\alivery.net\Debug\SqlBase.dll

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlBase
{
  public interface IRepository<T> where T : IValueObject, new()
  {
    Task<T> AddAsync(T model);

    Task<T> UpdateAsync(T model);

    Task<T> UpsertAsync(T model);

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

    Task<T> GetByIdAsync(string id);

    Task<T> FirstAsync(Expression<Func<T, bool>> predicate);
  }
}
