using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alivery
{
    class Configurations
    {
        public Configurations(IRepository<Configuration> db)
        {
            Application = new AppConfiguration(db);
            MessageQueue = new MessageQueueConfiguration(db);
        }

        public AppConfiguration Application { get; }
        public MessageQueueConfiguration MessageQueue { get; }

        public void OnFirstRun()
        {
            if (Application.FirstRun)
            {
                Application.SelfId = Guid.NewGuid().ToString();
                Application.FirstRun = false;
            }
        }

        public void Preconfigure()
        {
            MessageQueue.HostName = "localhost";
            MessageQueue.UserName = "user";
            MessageQueue.Password = "pass";
            MessageQueue.VirtualHost = "vh";

        }
    }
}
