using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SilentOrbit.Disk;

namespace SilentOrbit.Sync
{
    public class Stats
    {
        readonly Job job;
        readonly Queue queue;

        readonly int left;
        readonly int top;

        public Stats(Job job, Queue queue)
        {
            this.job = job;
            this.queue = queue;

            (left, top) = Console.GetCursorPosition();

            rateReporter = new Thread(RateReport);
            rateReporter.Name = "Rate Reporter";
        }

        /// <summary>
        /// Reports the progress once a seconds.
        /// Run this in it's own thread rather than using a timer
        /// because the threadpool will be full of other operations.
        /// </summary>
        readonly Thread rateReporter;

        #region Stopwatch

        readonly Stopwatch stopwatch = new Stopwatch();

        internal void Start()
        {
            stopwatch.Start();
            rateReporter.Start();
        }

        internal void Stop()
        {
            stopwatch.Stop();
            rateReporter.Interrupt();
        }

        #endregion

        #region Stats

        public int Nothing { get; private set; }
        public int New { get; private set; }
        public int Del { get; private set; }
        public int Att { get; private set; }
        public int Mod { get; private set; }
        public int Total => New + Del + Att + Mod + Nothing;

        internal void UpdateDel(FilePath t)
        {
            Del += 1;
            WriteDot("f", ConsoleColor.Red);
            WriteFileReport("File Del: " + (t - job.Target).PathRel);
        }

        internal void UpdateDel(DirPath t)
        {
            Del += 1;
            WriteDot("D", ConsoleColor.Red);
            WriteFileReport("Dir  Del: " + (t - job.Target).PathRel + "\\");
        }

        internal void UpdateNew(FilePath s)
        {
            New += 1;
            WriteDot("f", ConsoleColor.Green);
            WriteFileReport("File New: " + (s - job.Source).PathRel);
        }

        internal void UpdateNew(DirPath t)
        {
            WriteDot("D", ConsoleColor.DarkGreen);
            WriteFileReport("Dir  New: " + (t - job.Target).PathRel + "\\");
        }

        internal void UpdateMod(FilePath s)
        {
            Mod += 1;
            WriteDot("f", ConsoleColor.Yellow);
            WriteFileReport("File Mod: " + (s - job.Source).PathRel);
        }

        internal void UpdateNothing()
        {
            Nothing += 1;
            WriteDot(".", ConsoleColor.Black);
        }

        internal void UpdateAtt(FilePath _)
        {
            Att += 1;
            if (job.SyncAttributes)
            {
                WriteDot("a", ConsoleColor.DarkCyan);
                //WriteFileReport("File Att: " + (s - job.Source).PathRel);
            }
            else
            {
                WriteDot("a", ConsoleColor.Black);
            }
        }

        #endregion

        #region Report

        readonly object consoleLock = new object();

        bool writtenDot = false;

        void WriteDot(string text, ConsoleColor background)
        {
#if FALSE
            if (job.ReportEveryFile)
                return;

            lock (consoleLock)
            {
                writtenDot = true;
                Console.BackgroundColor = background;
                if (background == ConsoleColor.Black)
                    Console.ForegroundColor = ConsoleColor.Gray;
                else
                    Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(text);
            }
#endif
        }

        void WriteFileReport(string text)
        {
            if (job.ReportEveryFile == false)
                return;

            lock (consoleLock)
            {
                if (writtenDot)
                {
                    Console.ResetColor();
                    Console.WriteLine();
                }
                Console.WriteLine(text);
            }
        }

        void RateReport(object state)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    lock (consoleLock)
                    {
                        var r = Report();
                        Console.ResetColor();
                        Console.SetCursorPosition(left, top);
                        Console.WriteLine(r);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }

        public string Report()
        {
            var s = new StringBuilder();
            s.AppendLine();
            s.AppendLine("Runtime " + stopwatch.Elapsed);
            s.AppendLine("Queue: " + queue.Size);
            Report(s, "Changed", New + Mod + Del);
            Report(s, "New", New);
            Report(s, "Modified", Mod);
            Report(s, "Deleted", Del);
            s.AppendLine();
            Report(s, "Skipped", Att + Nothing);
            Report(s, "Attribute", Att);
            Report(s, "Unmodified", Nothing);

            return s.ToString();
        }

        void Report(StringBuilder s, string title, int value)
        {
            var seconds = stopwatch.Elapsed.TotalSeconds;
            var rate = value / seconds;

            s.AppendFormat("{0,-11} {1,8} {2,8}/s\n", title, value, rate.ToString("0.00"));
        }

#endregion
    }
}
