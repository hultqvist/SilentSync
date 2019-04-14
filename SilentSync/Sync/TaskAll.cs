using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    /// <summary>
    /// Collects task to be wrappen in a Task.WhenAll()
    /// </summary>
    public class TaskAll
    {
        readonly List<Task> tasks = new List<Task>();
        readonly Queue queue;

        public TaskAll(Queue queue)
        {
            this.queue = queue;
        }

        public void Run(Func<Task> action)
        {
            var t = queue.Run(action);
            tasks.Add(t);
        }

        /// <summary>
        /// Don't trigger run, assume they are already.
        /// </summary>
        public void Add(Task task)
        {
            tasks.Add(task);
        }

        public Task WhenAll()
        {
            return Task.WhenAll(tasks);
        }
    }

    public static class TaskAllExtensions
    {
        public static TaskAll WhenAll(this Queue queue)
        {
            return new TaskAll(queue);
        }
    }
}
