using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;

namespace Alivery.Net
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
