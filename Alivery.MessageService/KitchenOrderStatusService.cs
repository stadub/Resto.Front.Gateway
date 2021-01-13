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
        private OrderDatabase kitchenOrderDb;

        public KitchenOrderStatusService(OrderDatabase KitchenOrderDatabase)
        {
            kitchenOrderDb = KitchenOrderDatabase;
        }

        public async Task CreateKitchenOrderInfoAsync()
        {
            var kitchenOrdersToSend = await kitchenOrderDb.KitchenOrderTransmitStatus.GetAllAsync(KitchenOrder => KitchenOrder.TransmitStatus == TransmitStatus.Received || KitchenOrder.TransmitStatus == TransmitStatus.Unknown);

            foreach (var kitchenOrderToSend in kitchenOrdersToSend)
            {
                var oderId = kitchenOrderToSend.KitchenOrderId;

                var kitchenOrder = await kitchenOrderDb.KitchenOrder.GetByIdAsync(oderId);

                var kitchenOrderTransactionMessages = await kitchenOrderDb.KitchenOrderStatusMessage.GetAllAsync(x => x.IikoOrderId== kitchenOrder.IikoOrderId);

                //first status update for KitchenOrder
                if (kitchenOrderTransactionMessages == null)
                {
                    await kitchenOrderDb.KitchenOrderStatusMessage.AddAsync(new KitchenOrderStatusMessage
                    {
                        Revision = kitchenOrder.Revision,
                        OrderId = kitchenOrder.IikoOrderId,
                        OrderStatus = kitchenOrder.Status,
                        IikoOrderId = null,
                        Json = kitchenOrder.Json
                    });
                    continue;
                }


                //already has latest revision
                if (kitchenOrderTransactionMessages.Any(x => x.Revision == kitchenOrder.Revision))
                    continue;

                var initialRevision = kitchenOrderTransactionMessages.Min(x => x.Revision);


                var initial = kitchenOrderTransactionMessages.Find(message => message.Revision == initialRevision);

                var initialJson = JToken.Parse(initial.Json);
                var latestJson = JToken.Parse(kitchenOrder.Json);

                var diffJson = JsonDifferentiator.Differentiate(initialJson, latestJson);
                var KitchenOrderMsg = await kitchenOrderDb.KitchenOrderStatusMessage.AddAsync(new KitchenOrderStatusMessage
                {
                    Revision = kitchenOrder.Revision,
                    OrderId = kitchenOrder.IikoOrderId,
                    OrderStatus = kitchenOrder.Status,
                    IikoOrderId = null,
                    Json = diffJson.ToString()
                });


                //commit state update
                kitchenOrderToSend.KitchenOrderStatusMsgId = KitchenOrderMsg.Id;
                kitchenOrderToSend.TransmitStatus = TransmitStatus.ReadyToSend;

                await kitchenOrderDb.KitchenOrderTransmitStatus.UpdateAsync(kitchenOrderToSend);
            }

        }

        public async Task<List<(KitchenOrderTransmitStatus transmitStatus, KitchenOrderStatusMessage msg)>> GetKitchenOrderInfoAsync()
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
