using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alivery.Db;
using Alivery.Db.Model;
using JsonDiffer;
using Newtonsoft.Json.Linq;

namespace Alivery.MessageService
{
    class KitchenOrderStatusService
    {

        public async Task CreateKitchenOrderInfoAsync(OrderDatabase kitchenOrderDb)
        {
            var kitchenOrdersToSend = await kitchenOrderDb.KitchenOrderTransmitStatus.GetAllAsync(KitchenOrder => KitchenOrder.TransmitStatus == TransmitStatus.Received || KitchenOrder.TransmitStatus == TransmitStatus.Unknown);
            KitchenOrderStatusMessage KitchenOrderMsg;

            foreach (var kitchenOrderToSend in kitchenOrdersToSend)
            {
                var oderId = kitchenOrderToSend.KitchenOrderId;

                var kitchenOrder = await kitchenOrderDb.KitchenOrder.GetByIdAsync(oderId);

                var kitchenOrderTransactionMessages = await kitchenOrderDb.KitchenOrderStatusMessage.GetAllAsync(x => x.IikoOrderId== kitchenOrder.IikoOrderId);

                //first status update for KitchenOrder
                if (kitchenOrderTransactionMessages == null || !kitchenOrderTransactionMessages.Any())
                {
                    KitchenOrderMsg = await kitchenOrderDb.KitchenOrderStatusMessage.AddAsync(new KitchenOrderStatusMessage
                    {
                        OrderId = oderId,
                        IikoOrderId = kitchenOrder.IikoOrderId,
                        Json = kitchenOrder.Json
                    });
                    kitchenOrderToSend.KitchenOrderStatusMsgId = KitchenOrderMsg.Id;
                    kitchenOrderToSend.TransmitStatus = TransmitStatus.ReadyToSend;

                    await kitchenOrderDb.KitchenOrderTransmitStatus.UpdateAsync(kitchenOrderToSend);
                    continue;
                }

                //no revision changes found
                if (kitchenOrderTransactionMessages.Count == 1) 
                    continue;

               

                //var initialRevision = kitchenOrderTransactionMessages.Min(x => x.Revision);


                var initial = kitchenOrderTransactionMessages.First();

                var initialJson = JToken.Parse(initial.Json);
                var latestJson = JToken.Parse(kitchenOrder.Json);

                var diffJson = JsonDifferentiator.Differentiate(initialJson, latestJson);

                //no revision changes found
                if (diffJson == null || !diffJson.HasValues)
                    continue; ;


                KitchenOrderMsg = await kitchenOrderDb.KitchenOrderStatusMessage.AddAsync(new KitchenOrderStatusMessage
                {
                    OrderId = oderId,
                    IikoOrderId = kitchenOrder.IikoOrderId,
                    Json = diffJson.ToString()
                });


                //commit state update
                kitchenOrderToSend.KitchenOrderStatusMsgId = KitchenOrderMsg.Id;
                kitchenOrderToSend.TransmitStatus = TransmitStatus.ReadyToSend;

                await kitchenOrderDb.KitchenOrderTransmitStatus.UpdateAsync(kitchenOrderToSend);
            }

        }

        public async Task<List<(KitchenOrderTransmitStatus transmitStatus, KitchenOrderStatusMessage msg)>> GetKitchenOrderInfoAsync(OrderDatabase kitchenOrderDb)
        {
            var KitchenOrdersToSend = await kitchenOrderDb.KitchenOrderTransmitStatus.GetAllAsync(KitchenOrder => KitchenOrder.TransmitStatus == TransmitStatus.ReadyToSend);

            var messages = new List<(KitchenOrderTransmitStatus transmitStatus, KitchenOrderStatusMessage msg)>();

            foreach (var status in KitchenOrdersToSend)
            {
                var model = await kitchenOrderDb.KitchenOrderStatusMessage.GetByIdAsync(status.KitchenOrderStatusMsgId);
                messages.Add((status, model));
            }
            return messages;
        }

    }
}
