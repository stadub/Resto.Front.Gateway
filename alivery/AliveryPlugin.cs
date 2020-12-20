using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;

namespace alivery
{
    [UsedImplicitly]
    [PluginLicenseModuleId(21005108)]
    public sealed class AliveryPlugin : IFrontPlugin
    {
        Application app;

        public AliveryPlugin()
        {
            app = new Application();
            app.Start();
        }
        public void Dispose()
        {
            app.Dispose();
        }
    }

}
