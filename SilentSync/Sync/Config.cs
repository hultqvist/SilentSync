using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    public class Config
    {
        public List<string> Source { get; set; }
        public List<string> Target { get; set; }

        public List<string> Ignore { get; set; } = new List<string>();

        /// <summary>
        /// Sync attribute changes
        /// </summary>
        public bool SyncAttributes { get; set; }

        /// <summary>
        /// Write all changes to console as they happen.
        /// </summary>
        public bool ReportEveryFile { get; set; }

        public bool ThreadPoolQueue { get; set; }
    }
}
