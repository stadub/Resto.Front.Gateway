using System.IO;
using SqlBase;
using SQLite;

namespace SqliteDatabase
{
    public class SqliteDatabase: DatabaseBase<SQLiteAsyncConnection>
    {
        private SQLiteConnectionString options;

        public SqliteDatabase(string databasePath, string password) : base(databasePath, password)
        {
            if (!databasePath.Contains("\\"))
            {
                var curDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                databasePath = Path.Combine(curDir, databasePath + ".db");
            }

            options = new SQLiteConnectionString(databasePath, true,
                key: password,
                preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = OFF;"),
                postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"));
            
        }

        protected override SQLiteAsyncConnection CreateConnection()
        {
            return new SQLiteAsyncConnection(options);
        }

        protected override IDbProvider<SQLiteAsyncConnection> RegisterRepository<TValue>() 
        {
            return new SqliteRepository<TValue>();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection.CloseAsync().Wait();
            }
            base.Dispose(disposing);

        }
    }
}