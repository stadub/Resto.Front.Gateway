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
    class OrderStatusService
    {
        private OrderDatabase orderDb;

        public OrderStatusService(OrderDatabase orderDatabase)
        {
            orderDb = orderDatabase;
        }

        public async Task CreateOrderInfoAsync()
        {
            var ordersToSend = await orderDb.OrderTransmitStatus.GetAllAsync(order => order.TransmitStatus == TransmitStatus.Received || order.TransmitStatus == TransmitStatus.Unknown);
            OrderStatusMessage orderMsg;

            foreach (var orderToSend in ordersToSend)
            {
                var oderId = orderToSend.OrderId;

                var order = await orderDb.Order.GetByIdAsync(oderId);

                var orderTransactionMessages = await orderDb.OrderStatusMessage.GetAllAsync(x => x.IikoOrderId == order.IikoOrderId);

                //first status update for order
                if (orderTransactionMessages == null  || !orderTransactionMessages.Any() )
                {
                    orderMsg = await orderDb.OrderStatusMessage.AddAsync(new OrderStatusMessage
                    {
                        Revision = order.Revision,
                        OrderId = oderId,
                        OrderStatus = order.OrderStatus,
                        IikoOrderId = order.IikoOrderId,
                        Json = order.Json
                    });

                    orderToSend.OrderStatusMsgId = orderMsg.Id;
                    orderToSend.TransmitStatus = TransmitStatus.ReadyToSend;

                    await orderDb.OrderTransmitStatus.UpdateAsync(orderToSend);
                    continue;
                }

                var  woJson = orderTransactionMessages.Where(message => message.Json == null).ToList();
                foreach (var orderStatusMessage in woJson)
                {
                    var order1 = await orderDb.Order.GetByIdAsync(orderStatusMessage.OrderId);
                    orderStatusMessage.Json = order1.Json;
                    await orderDb.OrderStatusMessage.UpdateAsync(orderStatusMessage);
                }
                //already has latest revision
                if (orderTransactionMessages.Any(x => x.Revision == order.Revision))
                    continue;

                var initialRevision = orderTransactionMessages.Min(x => x.Revision);


                var initial = orderTransactionMessages.Find(message => message.Revision == initialRevision);

                var initialJson = JToken.Parse(initial.Json);
                var latestJson = JToken.Parse(order.Json);

                var diffJson = JsonDifferentiator.Differentiate(initialJson, latestJson);
                orderMsg = await orderDb.OrderStatusMessage.AddAsync(new OrderStatusMessage
                {
                    Revision = order.Revision,
                    OrderId = order.IikoOrderId,
                    OrderStatus = order.OrderStatus,
                    IikoOrderId = null,
                    Json = diffJson.ToString()
                });


                //commit state update
                orderToSend.OrderStatusMsgId = orderMsg.Id;
                orderToSend.TransmitStatus = TransmitStatus.ReadyToSend;

                await orderDb.OrderTransmitStatus.UpdateAsync(orderToSend);
            }

        }

        public async Task<List<(OrderTransmitStatus transmitStatus, OrderStatusMessage msg)>> GetOrderInfoAsync()
        {
            var ordersToSend = await orderDb.OrderTransmitStatus.GetAllAsync(order => order.TransmitStatus == TransmitStatus.ReadyToSend);

            var messages = new List<(OrderTransmitStatus transmitStatus, OrderStatusMessage msg)>();

            foreach (var status in ordersToSend)
            {
                var model = await orderDb.OrderStatusMessage.GetByIdAsync(status.OrderStatusMsgId);
                messages.Add((status, model));
            }
            return messages;
        }

    }
}
