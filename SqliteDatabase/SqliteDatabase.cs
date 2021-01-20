using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SqlBase;
using SQLite;
using Utils;

namespace SqliteDatabase
{

    [Guid("574e9cd2-1c17-4672-b530-ab6eb314d04e")]
    public class SqliteDatabase: DatabaseBase<SQLiteAsyncConnection>
    {
        private readonly string databasePath;

        private SQLiteConnectionString options;
        private FileStream mutex;
        public SqliteDatabase(string dbPath, string password) : base(dbPath, password)
        {
            this.databasePath = dbPath;

            if (!databasePath.Contains("\\"))
            {
                var curDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                databasePath = Path.Combine(curDir, databasePath + ".db");
            }

            options = new SQLiteConnectionString(dbPath, true,
                key: password,
                preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = OFF;"),
                postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"));


            //mutex = CreateLock(this, databasePath);
        }


        private static (FileStream mutex, bool locked) CreateLock(object @type, string databasePath)
        {
            bool locked = false;
            FileStream locker = null;
            var lockFile = databasePath + ".lock";
            try
            {
                if (!File.Exists(lockFile))
                {
                    File.Create(lockFile).Close();
                }

                locker = new FileStream(databasePath + ".lock", FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                // must include Write access in order to lock file
                locker.Lock(0, 0); // 0,0 has special meaning to lock entire file regardless of length 
            }
            catch (IOException e)
            {
                locked = true;
            }
           
            return (mutex: locker, locked: locked);
        }

        protected override async Task<SQLiteAsyncConnection> CreateConnectionAsync()
        {
            var locker = CreateLock(this, databasePath);
            mutex = locker.mutex;


            if (locker.locked)
            {
                for (int i = 0; i < 5 && locker.locked ;  i++)
                {
                    await Task.Delay(1000);
                    locker = CreateLock(this, databasePath);
                    mutex = locker.mutex;
                }

                if (locker.locked)
                {
                    throw new TimeoutException("unable to acquire access");
                }
            }

            var connection = new SQLiteAsyncConnection(options);
            return connection;
        }

        protected override IDbProvider<SQLiteAsyncConnection> RegisterRepository<TValue>() 
        {
            return new SqliteRepository<TValue>();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mutex.Close();
                mutex.Dispose();
                Connection.CloseAsync().Wait();
            }
            base.Dispose(disposing);

        }

        protected override async Task DisposeConnectionAsync()
        {
            mutex.Close();


            mutex.Dispose();
            await Connection.CloseAsync();
        }


    }
}