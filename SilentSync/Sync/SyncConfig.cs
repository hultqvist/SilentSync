using Newtonsoft.Json;
using SilentOrbit.Disk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    public static class SyncConfig
    {
        public static void Run(FilePath configPath)
        {
            var json = configPath.ReadAllText();
            var config = JsonConvert.DeserializeObject<Config>(json);

            foreach (var targetBasePath in config.Target)
            {
                foreach (var sourcePath in config.Source)
                {
                    var job = new Job();
                    job.Source = new DirPath(sourcePath);
                    job.Target = new DirPath(targetBasePath).CombineDir(job.Source.Name);
                    job.ReportEveryFile = config.ReportEveryFile;
                    job.SyncAttributes = config.SyncAttributes;
                    job.Queue = config.ThreadPoolQueue ? (Queue)new ThreadPoolQueue() : new SyncQueue();

                    SetIgnore(job.Ignore, config.Ignore);

                    DirectoryReplicator.Run(job);
                }
            }
        }

        static void SetIgnore(Ignore ignore, List<string> list)
        {
            foreach (var i in list)
            {
                if (i.EndsWith("\\") || i.EndsWith("/"))
                    ignore.IgnoreDirName(i.Trim('\\', '/'));
                else
                    ignore.IgnoreFileName(i);
            }
        }
    }
}
