using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace alivery
{
    public interface IRepository
    {
        void Init(SQLiteConnection database);
    }

    public interface IRepository<T>
    {
        int Add(T model);
        int Update(T model);
        int Upsert(T model);
        List<T> GetAll(Expression<Func<T, bool>> predicate);
        T GetById(string id);
        T First(Expression<Func<T, bool>> predicate);

    }

    public class Repository<T>: IRepository<T>, IRepository where T : ValueObject, new()
    {
        private SQLiteConnection db;

        public void Init(SQLiteConnection database)
        {
            this.db = database;

            db.CreateTable<T>();

        }

        public int Add(T model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
                model.Id = Guid.NewGuid().ToString();

            return db.Insert(model);
        }


        public int Update(T model)
        {
            return db.Update(model);
        }

        public int Upsert(T model)
        {
            var item = db.Find<T>(model.Id);
            return item!=null ? Update(model) :Add(model);
        }

        public List<T> GetAll(Expression<Func<T, bool>> predicate) 
        {
            var query = db.Table<T>().Where(predicate);

            var result = query.ToList();
            return result;
        }

        public T GetById(string id)
        {
            return db.Find<T>(id);
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            var result = db.Find(predicate);
            return result;
        }
    }
}
