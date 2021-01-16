﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using alivery;
using Alivery.Db;
using DbConfiguration;
using Microsoft.Extensions.Logging;

namespace Alivery.MessageService
{
    public class Application:IDisposable
    {
        private readonly ILogger logger;

        private ConfigDatabase configDb;
        private OrderDatabase orderDb;
        MessageQueue messageQueue;


        public Application(ILogger logger)
        {
            this.logger = logger;

            configDb = new ConfigDatabase("суперсекретный пароль");
            configDb.Open();

            var config = new ConfigRegistry(configDb.Configuration);
            //var config = new ConfigRegistry("msgService.cfg","суперсекретный пароль2");

            var file = Assembly.GetExecutingAssembly();
            config.SyncFromConfigFile(file.Location);
            config.OnFirstRun();

            orderDb = new OrderDatabase(config.Application.OrderDbPath);

            messageQueue = new MessageQueue(config.OrderMessageQueue, config.KitchenOrderMessageQueue, orderDb);

        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public Application Start()
        {
            EntryPoint().Wait();
            
            logger.LogInformation("CookingPriorityManager started");
            return this;
        }

        private async Task EntryPoint()
        {
            logger.LogInformation("Start init...");


            orderDb.Open();



            while (true)
            {
                await Task.Delay(1000);
                if (disposed)
                    break;


                await messageQueue.CreateOrderMsg();

                await messageQueue.SendStatusUpdatesAsync();

            }
            logger.LogInformation("Exit...");

        }

       
        
        private bool disposed;


        public void Dispose()
        {
            if (disposed)
                return;
            configDb.Close();
            orderDb.Close();
            messageQueue.Dispose();
            disposed = true;
        }
    }
}
