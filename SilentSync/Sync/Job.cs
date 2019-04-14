using SilentOrbit.Disk;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit.Sync
{
    public class Job
    {
        public DirPath Source { get; set; }
        public DirPath Target { get; set; }

        public Ignore Ignore { get; } = new Ignore();

        /// <summary>
        /// Sync attribute changes
        /// </summary>
        public bool SyncAttributes { get; set; } = false;

        /// <summary>
        /// Write all changes to console as they happen.
        /// </summary>
        public bool ReportEveryFile { get; set; } = false;

        public Queue Queue { get; set; } = new ThreadPoolQueue();
    }
}
