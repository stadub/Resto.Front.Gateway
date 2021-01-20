using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using alivery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Microsoft.Extensions.Logging.Console;
using Utils;

namespace Alivery.MessageService
{
    class Program
    {

        //static Task Main(string[] args) =>
        //    CreateHostBuilder(args).Build().RunAsync();


        //static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
        //        .ConfigureLogging(builder =>
        //            builder.AddJsonConsole(options =>
        //            {
        //                options.IncludeScopes = false;
        //                options.TimestampFormat = "hh:mm:ss ";
        //                options.JsonWriterOptions = new JsonWriterOptions
        //                {
        //                    Indented = true
        //                };
        //            }));

        static void Main(string[] args)
        {
            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));

            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();


            using var configDb = new ConfigDatabase("суперсекретный пароль");
            configDb.OpenAsync().Wait();

            var config = new ConfigRegistry(configDb.Configuration);
            //var config = new ConfigRegistry("msgService.cfg","суперсекретный пароль2");

            var file = Assembly.GetExecutingAssembly();
            config.SyncFromConfigFile(file.Location);
            config.OnFirstRun();


            //Runtime.Exclusive(config.Application.SelfId,() =>
            //{
                new Application(config, logger).EntryPoint().Wait();

            //});
        }

        
    }
}
