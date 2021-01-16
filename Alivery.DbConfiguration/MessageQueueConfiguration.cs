using DbConfiguration;
using DbConfiguration.Models;
using SqlBase;

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


        public string QueueName
        {
            get => ReadConfig("QueueName");
            set => WriteConfig("QueueName", value);
        }

        public int Port
        {
            get => ReadConfig<int>("Port");
            set => WriteConfig("Port", value);
        }

        public MessageQueueConfiguration(string configType) : base( configType)
        {
        }
    }


}
