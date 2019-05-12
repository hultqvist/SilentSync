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
    /// Relative path to a file.
    /// When combined with a DirPath it will result in a FilePath
    /// </summary>
    public class RelFilePath : RelDiskPath
    {
        public RelFilePath(string path) : base(path)
        {

        }
    }
}
