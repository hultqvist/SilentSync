using SilentOrbit.Disk;
using SilentOrbit.Sync;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SilentOrbit
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
SilentSync.exe <source path> <destination path>
One direction sync from source to destination, files in destination are copied and deleted to match those in source.
"
);

            if (args.Length == 0)
                return;

            try
            {
                Sync(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Error.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Console.ReadLine();
            }
        }

        static void Sync(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Expected 2 arguments");

            var job = new Job();
            job.Source = (DirPath)args[0];
            job.Target = (DirPath)args[1];

            var ignore = job.Ignore;
            ignore.IgnoreFileName("desktop.ini");
            ignore.IgnoreFileNameRegex(@"Synctoy.*\.dat");
            ignore.IgnoreDirName("bin");
            ignore.IgnoreDirName("obj");
            ignore.IgnoreDirName(".vs");

            Stats stats = DirectoryReplicator.Run(job);

            Console.WriteLine(stats.Report());
            Console.WriteLine();
        }
    }
}
