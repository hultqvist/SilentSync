using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SilentOrbit.Tools
{
    public static class RetryExtensions
    {
        #region Retry helpers

        public static void Retry(this Action action)
        {
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Retrying... " + ex.Message);
                    Thread.Sleep(500);
                }
            }
        }

        public static T Retry<T>(this Func<T> func)
        {
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(500);
                }
            }
        }

        #endregion
    }
}
