using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SilentOrbit.Disk
{
    /// <summary>
    /// Full path to a directory
    /// </summary>
    public partial class DirPath : FullDiskPath
    {
        #region Static

        public static DirPath GetCurrentDirectory() => (DirPath)Directory.GetCurrentDirectory();

        #endregion

        public readonly DirectoryInfo DirectoryInfo;

        public DirPath(DirectoryInfo info) : base(info.FullName)
        {
            this.DirectoryInfo = info;
        }

        public DirPath(string path) : this(new DirectoryInfo(path))
        {
        }

        //Only explicit
        public static explicit operator DirPath(string value)
        {
            return new DirPath(value);
        }

        #region Path operations

        public override DirPath Parent
        {
            get
            {
                var parent = DirectoryInfo.Parent;
                if (parent == null)
                    return null;
                return new DirPath(parent);
            }
        }

        public DirPath CombineDir(params string[] parts)
        {
            string path = PathFull;
            foreach (var p in parts)
            {
                path = Path.Combine(path, p);
            }

            return new DirPath(path);
        }

        public FilePath CombineFile(params string[] parts)
        {
            string path = PathFull;
            foreach (var p in parts)
            {
                path = Path.Combine(path, p.Replace('/', '\\'));
            }

            return new FilePath(path);
        }

        #endregion

        #region Directory properties

        public override bool Exists() => Directory.Exists(LongPathFull);

        public override string Name
        {
            get
            {
                if (DirectoryInfo.Parent == null)
                    return PathFull;
                else
                    return base.Name;
            }
        }

        #endregion

        #region Directory list

        public IEnumerable<DirPath> GetDirectories()
        {
            return GetDirectories(DirectoryInfo.EnumerateDirectories());
        }

        public IEnumerable<DirPath> GetDirectories(string pattern)
        {
            return GetDirectories(DirectoryInfo.EnumerateDirectories(pattern));
        }

        public IEnumerable<DirPath> GetDirectories(string pattern, SearchOption searchOption)
        {
            return GetDirectories(DirectoryInfo.EnumerateDirectories(pattern, searchOption));
        }

        static IEnumerable<DirPath> GetDirectories(IEnumerable<DirectoryInfo> dirs)
        {
            foreach (var d in dirs)
                yield return new DirPath(d);
        }

        public IEnumerable<FilePath> GetFiles()
        {
            return GetFiles(DirectoryInfo.EnumerateFiles());
        }

        public IEnumerable<FilePath> GetFiles(string pattern)
        {
            return GetFiles(DirectoryInfo.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
        }

        public IEnumerable<FilePath> GetFiles(string pattern, SearchOption searchOption)
        {
            return GetFiles(DirectoryInfo.EnumerateFiles(pattern, searchOption));
        }

        static IEnumerable<FilePath> GetFiles(IEnumerable<FileInfo> files)
        {
            foreach (var f in files)
                yield return new FilePath(f);
        }

        #endregion

        #region Directory operations

        public void CreateDirectory()
        {
            Retry(() => Directory.CreateDirectory(LongPathFull));
        }

        /// <summary>
        /// Delete all files and folders inside
        /// but leave the root folder intact
        /// </summary>
        public void EmptyDirectory()
        {
            if (Exists() == false)
            {
                Directory.CreateDirectory(LongPathFull);
                return;
            }

            Retry(() =>
            {
                foreach (var f in GetFiles("*", SearchOption.AllDirectories))
                {
                    //if (File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly) == false)
                    //    continue;

                    f.SetAttributes(FileAttributes.Normal);
                    f.DeleteFile();
                }

                foreach (var d in Directory.GetDirectories(LongPathFull))
                {
                    Directory.Delete(d, true);
                }
            });
        }

        /// <summary>
        /// Return number of files copied
        /// </summary>
        /// <param name="target"></param>
        /// <param name="overwrite"></param>
        /// <returns>Number of files copied</returns>
        public int CopyDirectory(DirPath target)
        {
            int files = 0;

            target.CreateDirectory();
            foreach (var f in GetFiles())
            {
                f.CopyTo(target.CombineFile(f.FileName));
                files += 1;
            }
            foreach (var d in GetDirectories())
                files += d.CopyDirectory(target.CombineDir(d.Name));

            return files;
        }

        public void DeleteDir()
        {
            if (File.Exists(PathFull))
                throw new InvalidOperationException("Expected a directory, found a file");

            if (Directory.Exists(LongPathFull))
                Directory.Delete(LongPathFull, recursive: true);
        }

        public void DeleteEmptyDir()
        {
            Directory.Delete(LongPathFull, recursive: true);
        }

        #endregion

        #region RelPath operations

        public static RelFilePath operator -(FilePath path, DirPath root)
        {
            if (path.PathFull.StartsWith(root.PathFull) == false)
                throw new ArgumentException("path must be in the Source directory");

            var rel = path.PathFull.Substring(root.PathFull.Length).TrimStart('\\');
            return new RelFilePath(rel);
        }


        public static FilePath operator +(DirPath root, RelFilePath rel)
        {
            return new FilePath(Path.Combine(root.PathFull, rel.PathRel));
        }


        public static RelDirPath operator -(DirPath path, DirPath root)
        {
            Debug.Assert(path.PathFull.StartsWith(root.PathFull));
            if (path.PathFull.StartsWith(root.PathFull) == false)
            {
                throw new ArgumentException("path must be in the Source directory");
            }

            var rel = path.PathFull.Substring(root.PathFull.Length).TrimStart('\\');
            return new RelDirPath(rel);
        }

        public static DirPath operator +(DirPath root, RelDirPath rel)
        {
            return new DirPath(Path.Combine(root.PathFull, rel.PathRel));
        }

        #endregion
    }
}
