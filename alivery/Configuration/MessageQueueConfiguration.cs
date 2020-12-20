using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace alivery
{
    public class MessageQueueConfiguration: ConfigurationBase
    {

        public string HostName
        {
            get => ReadConfig("HostName", "localhost");
            set => WriteConfig("HostName", value);
        }

        public string UserName
        {
            get => ReadConfig("UserName");
            set => WriteConfig("UserName", value);
        }
        public string Password
        {
            get => ReadConfig("Password");
            set => WriteConfig("Password", value);
        }
        public string VirtualHost
        {
            get => ReadConfig("VirtualHost");
            set => WriteConfig("VirtualHost", value);
        }


        public string QueueName
        {
            get => ReadConfig("QueueName");
            set => WriteConfig("QueueName", value);
        }

        public MessageQueueConfiguration(IRepository<Configuration> db) : base(db)
        {
        }
    }
}
