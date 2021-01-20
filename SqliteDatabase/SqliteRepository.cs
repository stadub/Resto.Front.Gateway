using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlBase;
using SQLite;

namespace SqliteDatabase
{
    public class SqliteRepository<T>: IRepository<T>, IDbProvider<SQLiteAsyncConnection> where T : IValueObject, new()
    {
        private SQLiteAsyncConnection db;

        public async Task InitAsync(SQLiteAsyncConnection database)
        {
            this.db = database;

            var info = await db.GetTableInfoAsync(typeof(T).Name);

            if (!info.Any())
              await db.CreateTableAsync<T>();

        }

        public async Task<T> AddAsync(T model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
                model.Id = Guid.NewGuid().ToString();

            var result =  await db.InsertAsync(model);
            return result > 0 ? model : default;
        }


        public async Task<T> UpdateAsync(T model)
        {
            var result = await db.UpdateAsync(model);
            return result > 0 ? model : default;
        }

        public async Task<T> UpsertAsync(T model)
        {
            var item = await db.FindAsync<T>(model.Id);
            return await (item!=null ? UpdateAsync(model) : AddAsync(model));
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) 
        {
            var query = db.Table<T>().Where(predicate);

            var result = query.ToListAsync();
            return await result;
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
