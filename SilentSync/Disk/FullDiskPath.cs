using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace SilentOrbit.Disk
{
    /// <summary>
    /// Base class for FilePath and DirPath
    /// </summary>
    public abstract class FullDiskPath : IComparable
    {
        public readonly string PathFull;

        protected FullDiskPath(string value)
        {
            if (value.EndsWith(":\\"))
            {
                //Keep "C:\\"
            }
            else
            {
                value = value.TrimEnd('\\');
            }

            var full = Path.GetFullPath(value);
            if (full != value)
                throw new ArgumentException("Expected a full path: " + value + " != " + full);

            PathFull = value;
        }

        #region Path string operations

        /// <summary>
        /// File or directory name.
        /// </summary>
        public virtual string Name
        {
            get
            {
                var n = Path.GetFileName(PathFull);
                if (string.IsNullOrEmpty(n))
                    throw new InvalidProgramException();
                return n;
            }
        }

        public bool StartsWith(FullDiskPath test)
        {
            return PathFull.StartsWith(test.PathFull);
        }

        public bool EndsWith(string name)
        {
            return PathFull.EndsWith(name);
        }

        /// <summary>
        /// Length of path string.
        /// </summary>
        public int Length => PathFull.Length;

        public override string ToString() => PathFull;

        #endregion

        public virtual DirPath Parent => new DirPath(Path.GetDirectoryName(PathFull));

        #region Comparison

        public static bool operator ==(FullDiskPath a, FullDiskPath b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;

            Debug.Assert(a.GetType() == b.GetType());

            return a.PathFull == b.PathFull;
        }

        public static bool operator !=(FullDiskPath a, FullDiskPath b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return ((FullDiskPath)obj).PathFull == PathFull;
        }

        public override int GetHashCode()
        {
            return PathFull.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            //Assume it's the right type, don't handle compare with other types.
            var o = (FullDiskPath)obj;
            return PathFull.CompareTo(o.PathFull);
        }

        #endregion

        #region Retry helpers

        protected static void Retry(Action action)
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

        protected static T Retry<T>(Func<T> func)
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
