using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SilentOrbit.Disk;

namespace SilentOrbit.Sync
{
    /// <summary>
    /// Queue of all tasks
    /// </summary>
    public abstract class Queue
    {
        /// <summary>
        /// Number of parallell or waiting tasks
        /// </summary>
        public abstract int Size { get; }

        public Task Run(Action action)
        {
            return Run(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        public abstract Task Run(Func<Task> action);

        public abstract void Wait();
    }
}
