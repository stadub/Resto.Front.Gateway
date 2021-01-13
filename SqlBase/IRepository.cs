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