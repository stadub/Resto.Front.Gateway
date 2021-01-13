using System;
using System.Collections.Generic;

namespace SqlBase
{
    public abstract class DatabaseBase<TDatabase>:IDisposable
    {
        private bool disposed;
        protected TDatabase Connection { get; private set; }

        
        protected Dictionary<string,IDbProvider<TDatabase>> Tables { get;  }

        public DatabaseBase(string name, string password)
        {
            Tables = new Dictionary<string, IDbProvider<TDatabase>>();
        }


        public void Open()
        {
            // Get an absolute path to the database file
            Connection = CreateConnection();

            foreach (var table in Tables)
            {
                table.Value.InitAsync(Connection);
            }
        }

        protected abstract TDatabase CreateConnection();


        protected IRepository<TValue> RegisterTable<TValue>() where TValue : IValueObject, new()
        {
            var table = RegisterRepository<TValue>();
            var model = typeof(TValue);
            Tables.Add(model.Name, table);
            return (IRepository<TValue>) table;
        }

        protected abstract IDbProvider<TDatabase> RegisterRepository<TValue>() where TValue : IValueObject,new();

        public void Close()
        {
            Dispose();
        }



        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            Dispose(true);
            GC.SuppressFinalize(this);
            disposed = true;

        }
    }
}
