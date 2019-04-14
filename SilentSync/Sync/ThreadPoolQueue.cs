using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    class ThreadPoolQueue : Queue
    {
        readonly ManualResetEvent queueEmpty = new ManualResetEvent(false);

        /// <summary>
        /// Abort all operations
        /// </summary>
        bool abortAllWork = false;

        public ThreadPoolQueue()
        {
        }

        public override int Size => poolQueueSize;

        readonly object poolQueueSizeLock = new object();
        int poolQueueSize = 0;

        public override Task Run(Func<Task> action)
        {
            lock (poolQueueSizeLock)
            {
                poolQueueSize += 1;
                queueEmpty.Reset();
            }

            return Task.Run(async () =>
            {
                try
                {
                    if (abortAllWork)
                        return;

                    await action();
                }
                catch (Exception ex)
                {
                    abortAllWork = true;

                    Debug.Fail(ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
                    Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    lock (poolQueueSizeLock)
                    {
                        poolQueueSize -= 1;
                        Debug.Assert(poolQueueSize >= 0);

                        if (poolQueueSize == 0)
                            queueEmpty.Set();
                    }
                }
            });
        }

        public override void Wait()
        {
            queueEmpty.WaitOne();
        }
    }
}
