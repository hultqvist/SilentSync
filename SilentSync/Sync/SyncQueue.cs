using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    class SyncQueue : Queue
    {
        public override int Size => 0;

        public override Task Run(Func<Task> action)
        {
            var task = action();
            task.Wait();
            return task;
        }

        public override void Wait()
        {

        }
    }
}
