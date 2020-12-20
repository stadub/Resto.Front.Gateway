using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alivery
{
    class PrepareConfig
    {
        private readonly AppConfiguration appConfig;
        private readonly MessageQueueConfiguration msq;

        public PrepareConfig(AppConfiguration appConfig, MessageQueueConfiguration msq)
        {
            this.appConfig = appConfig;
            this.msq = msq;
        }
        public void OnFirstRun()
        {
            if (appConfig.FirstRun)
            {
                appConfig.SelfId = Guid.NewGuid().ToString();
            }
        }


        public void Preconfigure()
        {
            msq.HostName = "localhost";
            msq.UserName = "user";
            msq.Password = "pass";
            msq.VirtualHost = "vh";

        }
    }
}
