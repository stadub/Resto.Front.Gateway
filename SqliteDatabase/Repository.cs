using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;

namespace SqliteDatabase
{
    public interface IRepository
    {
        Task InitAsync(SQLiteAsyncConnection database);
    }

    public interface IRepository<T>
    {
        Task<int> AddAsync(T model);
        Task<int> UpdateAsync(T model);
        Task<int> UpsertAsync(T model);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(string id);
        Task<T> FirstAsync(Expression<Func<T, bool>> predicate);

    }

    public class Repository<T>: IRepository<T>, IRepository where T : ValueObject, new()
    {
        private SQLiteAsyncConnection db;

        public async Task InitAsync(SQLiteAsyncConnection database)
        {
            this.db = database;

            await db.CreateTableAsync<T>();

        }

        public async Task<int> AddAsync(T model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
                model.Id = Guid.NewGuid().ToString();

            return await db.InsertAsync(model);
        }


        public async Task<int> UpdateAsync(T model)
        {
            return await db.UpdateAsync(model);
        }

        public async Task<int> UpsertAsync(T model)
        {
            var item = await db.FindAsync<T>(model.Id);
            return await (item!=null ? UpdateAsync(model) : AddAsync(model));
        }

        public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) 
        {
            var query = db.Table<T>().Where(predicate);

            var result = query.ToListAsync();
            return result;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await db.FindAsync<T>(id);
        }

        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            var result = await db.FindAsync(predicate);
            return result;
        }
    }
}
