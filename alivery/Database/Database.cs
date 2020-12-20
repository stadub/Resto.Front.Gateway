using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace alivery
{
    public class Database
    {
        string curDir;
        SQLiteConnection db;
        private SQLiteConnectionString options;
        public Database()
        {
            curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var databasePath = Path.Combine(curDir, "alivery.db");


            options = new SQLiteConnectionString(databasePath, true,
                key: "суперсекретный пароль",
                preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = OFF;"),
                postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"));
        }
        public void Init()
        {
            // Get an absolute path to the database file
            


            db = new SQLiteConnection(options);

            db.CreateTable<OrderQueue>();
            db.CreateTable<Configuration>();

        }

        public void ShootDown()
        {
            db.Close();
        }


        public int Store<T>(T model)
        {
            return db.Insert(model);
        }

        public List<T> Read<T>(Expression<Func<T,bool>> predicate) where T : new()
        {
            var query = db.Table<T>().Where(predicate);

            var result = query.ToList();
            return result;
        }
    }
}
