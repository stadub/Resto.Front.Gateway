
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alivery
{
    public class Application:IDisposable
    {
        private AppConfiguration appConfig;
        private MessageQueueConfiguration msq;
        private Database db;

        public Application()
        {
            db = new Database();
            this.appConfig = new AppConfiguration(db);
            this.msq = new MessageQueueConfiguration(db);
        }

        public void Init()
        {


        }

        public void ReceiveFromFront()
        {

        }

        public void Dispose()
        {
            db.ShootDown();
        }
    }
}
