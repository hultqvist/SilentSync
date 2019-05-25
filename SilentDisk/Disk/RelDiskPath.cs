using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SilentOrbit.Disk
{
    /// <summary>
    /// Relative path within a filesystem to either a file or directory.
    /// </summary>
    public class RelDiskPath
    {
        public readonly string PathRel;

        public RelDiskPath(string relPath)
        {
            relPath = relPath.Replace('/', '\\');
            
            if (relPath.StartsWith("\\"))
                throw new ArgumentException("Rel paths can't start with \\");

            PathRel = relPath;
        }

        public static explicit operator RelDiskPath(string value)
        {
            return new RelDiskPath(value);
        }

        #region String operations

        public string Name => Path.GetFileName(PathRel);

        public override string ToString() => PathRel;

        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(PathRel);

        public bool StartsWith(string relPath) => PathRel.StartsWith(relPath);

        #endregion

        #region Path operations

        public static RelDiskPath operator +(RelDiskPath a, RelDiskPath b)
        {
            var path = Path.Combine(a.PathRel, b.PathRel);
            return new RelDiskPath(path);
        }

        public RelDirPath Parent => new RelDirPath(Path.GetDirectoryName(PathRel));

        public RelDiskPath Combine(params string[] parts)
        {
            var path = this;
            foreach (var p in parts)
            {
                path += new RelDiskPath(p);
            }

            return new RelDirPath(path.PathRel);
        }

        public RelDirPath CombineDir(params string[] parts) => new RelDirPath(Combine(parts).PathRel);
        public RelFilePath CombineFile(params string[] parts) => new RelFilePath(Combine(parts).PathRel);

        #endregion

        #region Comparison

        public static bool operator ==(RelDiskPath a, RelDiskPath b)
        {
            if (b is null && a is null)
            {
                return true;
            }

            if (b is null || a is null)
            {
                return false;
            }

            return a.PathRel == b.PathRel;
        }

        public static bool operator !=(RelDiskPath a, RelDiskPath b)
        {
            if (b is null && a is null)
            {
                return false;
            }

            if (b is null || a is null)
            {
                return true;
            }

            return a.PathRel != b.PathRel;
        }

        public override bool Equals(object obj)
        {
            return ((RelDiskPath)obj).PathRel == PathRel;
        }

        public override int GetHashCode()
        {
            return PathRel.GetHashCode();
        }

        #endregion
    }
}
