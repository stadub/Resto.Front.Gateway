using Utils;
using Utils.Models;
using SqlBase;

namespace alivery
{
    public class AppConfiguration : ConfigurationBase
    {
        public AppConfiguration() : base( "app")
        {
        }

        public string SelfId
        {
            get => ReadConfig("SelfId");
            set => WriteConfig("SelfId", value);
        }


        public bool FirstRun
        {
            get => ReadConfig<bool>("FirstRun", true);
            set => WriteConfig("FirstRun", value);
        }

        public string MsgServicePath
        {
            get => ReadConfig("MsgServicePath");
            set => WriteConfig("MsgServicePath", value);
        }

        public string OrderDbPath
        {
            get => ReadConfig("OrderDbPath");
            set => WriteConfig("OrderDbPath", value);
        }
    }
}