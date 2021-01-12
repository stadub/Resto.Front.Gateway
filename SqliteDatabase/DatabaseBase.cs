using System;
using System.Collections.Generic;
using System.IO;
using SQLite;

namespace SqliteDatabase
{
    public class DatabaseBase:IDisposable
    {
        private bool disposed;
        protected SQLiteAsyncConnection Connection { get; private set; }

        private SQLiteConnectionString options;
        protected Dictionary<string,IRepository> Tables { get;  }

        public DatabaseBase(string name, string password)
        {
            var curDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var databasePath = Path.Combine(curDir, name+".db");


            options = new SQLiteConnectionString(databasePath, true,
                key: password,
                preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = OFF;"),
                postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"));
            Tables = new Dictionary<string, IRepository>();
        }


        public void Open()
        {
            // Get an absolute path to the database file

            Connection = new SQLiteAsyncConnection(options);
            foreach (var table in Tables)
            {
                table.Value.InitAsync(Connection);
            }
        }


        protected IRepository<TValue> RegisterTable<TValue>() where TValue : ValueObject, new()
        {
            var table = new Repository<TValue>();
            var model = typeof(TValue);
            Tables.Add(model.Name, table);
            return table;
        }

        public void Close()
        {
            Dispose();
        }


        public void Dispose()
        {
            if (disposed)
                return;
            Connection.CloseAsync().Wait();
            disposed = true;
        }
    }
}
