using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.Disk
{
    /// <summary>
    /// Relative path to a directory.
    /// When combined with a DirPath it will result in a DirPath
    /// </summary>
    public class RelDirPath : RelDiskPath
    {
        public RelDirPath(string path) : base(path)
        {

        }

        public RelFilePath Combine(params RelFilePath[] parts)
        {
            var path = this.PathRel;
            foreach (var p in parts)
                path = Path.Combine(path, p.PathRel);

            return new RelFilePath(path);
        }

    }
}
