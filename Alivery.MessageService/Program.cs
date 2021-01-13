using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using alivery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Alivery.MessageService
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger= NullLogger.Instance;
            Exclusive(() =>
            {
                new Application(logger).Start();
                Thread.Sleep(TimeSpan.FromSeconds(5));

            });
        }

        protected static void Exclusive(Action action)
        {
            string mutexId = $"Global\\{{{AppId()}}}";


            using var mutex = new Mutex(false, mutexId, out var createdNew);
            // edited by acidzombie24
            var hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(5000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access");
                }
                catch (AbandonedMutexException)
                {
                    // Log the fact that the mutex was abandoned in another process,
                    // it will still get acquired
                    hasHandle = true;
                }

                action();
            }
            finally
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
            }
        }

        protected static string AppId()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var appGuid = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            return appGuid;
        }
    }
}
