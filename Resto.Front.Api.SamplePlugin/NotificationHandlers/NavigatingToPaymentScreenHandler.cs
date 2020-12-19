using System;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Organization;
using Resto.Front.Api.Data.Payments;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.OperationContexts;
using Resto.Front.Api.UI;

namespace Resto.Front.Api.SamplePlugin.NotificationHandlers
{
    internal sealed class NavigatingToPaymentScreenHandler : IDisposable
    {
        private readonly IDisposable subscription;

        public NavigatingToPaymentScreenHandler()
        {
            subscription = PluginContext.Notifications.SubscribeOnNavigatingToPaymentScreen(OnNavigatingToPaymentScreen);
        }

        private static void OnNavigatingToPaymentScreen(IOrder order, IPointOfSale pos, IOperationService os, IViewManager vm, INavigatingToPaymentScreenOperationContext context)
        {
            var currentInfo = context.ChequeAdditionalInfo; // other plugins could set another values, try to respect them
            PluginContext.Log.Info($"{(currentInfo == null ? "Providing" : "Overriding")} cheque additional info...");

            context.ChequeAdditionalInfo = new ChequeAdditionalInfo(
                currentInfo?.NeedReceipt ?? true,
                currentInfo?.Phone,
                "mail@example.com",
                currentInfo?.SettlementPlace);
        }

        public void Dispose() => subscription.Dispose();
    }
}
