using System;
using System.Reflection;
using System.Threading;

namespace Utils
{
    public class Runtime
    {
        public static void Exclusive(string appId, Action action)
        {
            var mutexId = $"Global\\{{{appId}}}";

            using var mutex = new Mutex(false, mutexId, out var createdNew);
            // edited by acidzombie24
            var hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(10000, false);
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
    }
}
