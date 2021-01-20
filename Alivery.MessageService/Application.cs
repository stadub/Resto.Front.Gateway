using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using alivery;
using Alivery.Db;
using Utils;
using Microsoft.Extensions.Logging;

namespace Alivery.MessageService
{
    public class Application:IDisposable
    {
        private readonly ConfigRegistry config;
        private readonly ILogger logger;

        MessageQueue messageQueue;


        public Application(ConfigRegistry config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }



        public async Task EntryPoint()
        {
            Debugger.Launch();
            logger.LogInformation("Start init...");

            messageQueue = new MessageQueue( config,config.OrderMessageQueue, config.KitchenOrderMessageQueue);

            while (true)
            {
                await Task.Delay(5000);
                if (disposed)
                    break;

                try
                {
                    var orderDb = new OrderDatabase(config.Application.OrderDbPath);
                    using (var db = await orderDb.OpenAsync())
                    {

                        await messageQueue.CreateOrderMsg(orderDb);

                        await messageQueue.SendStatusUpdatesAsync(orderDb);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e,"error from Message service");
                }
                

            }
            logger.LogInformation("Exit...");

        }

       
        
        private bool disposed;


        public void Dispose()
        {
            if (disposed)
                return;
            
            messageQueue.Dispose();
            disposed = true;
        }
    }
}
