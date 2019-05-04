using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SilentOrbit.Disk
{
    /// <summary>
    /// Full path to a file
    /// </summary>
    public class FilePath : FullDiskPath
    {
        public readonly FileInfo FileInfo;

        public FilePath(FileInfo info) : base(info.FullName)
        {
            this.FileInfo = info;
        }

        public FilePath(string path) : this(new FileInfo(path))
        {
        }

        //Only explicit
        public static explicit operator FilePath(string value)
        {
            return new FilePath(value);
        }

        #region File operations

        public override bool Exists() => File.Exists(PathFull);

        public void ClearReadOnly()
        {
            if (FileInfo.Exists == false)
                return;

            var a = FileInfo.Attributes;
            if ((a & FileAttributes.ReadOnly) != 0)
            {
                FileInfo.Attributes = a & ~FileAttributes.ReadOnly;
            }
        }

        public void SetAttributes(FileAttributes fileAttributes)
        {
            FileInfo.Attributes = fileAttributes;
        }

        public void DeleteFile()
        {
            ClearReadOnly();
            File.Delete(PathFull);
        }

        #endregion

        #region PathOperations

        public override DirPath Parent => new DirPath(FileInfo.Directory);

        public FilePath AppendPath(string text)
        {
            return new FilePath(PathFull + text);
        }

        /// <summary>
        /// Replace the current extension with a new extension.
        /// </summary>
        /// <param name="newExtension"></param>
        /// <returns></returns>
        public FilePath GetWithExtension(string newExtension)
        {
            Debug.Assert(newExtension == "" || newExtension.StartsWith("."));

            var dir = Path.GetDirectoryName(PathFull);
            var file = Path.GetFileNameWithoutExtension(PathFull);
            var disk = new FilePath(Path.Combine(dir, file + newExtension));
            return disk;
        }

        public FilePath ReplaceEnd(string expectedEnd, string newEnd)
        {
            if (PathFull.EndsWith(expectedEnd))
                return new FilePath(PathFull.Substring(0, PathFull.Length - expectedEnd.Length) + newEnd);
            else
                throw new ArgumentException("Path does not end in " + expectedEnd + " path: " + PathFull);
        }

        #endregion

        #region String operations

        public string FileName => Name;
        
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(PathFull);

        #endregion

        #region File content operations

        public byte[] ReadAllBytes()
        {
            return Retry(() => File.ReadAllBytes(PathFull));
        }

        public string ContentHash()
        {
            var content = ReadAllBytes();
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(sha1.ComputeHash(content));
            }
        }

        public void WriteAllBytes(byte[] data)
        {
            Parent.CreateDirectory();
            File.WriteAllBytes(PathFull, data);
        }

        public void WriteAllBytesRO(byte[] data)
        {
            Parent.CreateDirectory();

            Retry(() =>
            {
                //Read only generated files
                if (Exists())
                {
                    File.SetAttributes(PathFull, FileAttributes.Normal);
                }

                File.WriteAllBytes(PathFull, data);
                SetAttributes(FileAttributes.ReadOnly);
            });
        }

        public void Move(FilePath target) => File.Move(PathFull, target.PathFull);

        public string ReadAllText() => File.ReadAllText(PathFull, Encoding.UTF8);

        public void WriteAllTextRO(string text)
        {
            Parent.CreateDirectory();

            //Read only generated files
            if (Exists())
                FileInfo.Attributes = FileAttributes.Normal;

            File.WriteAllText(PathFull, text, new UTF8Encoding(false, true));
            SetAttributes(FileAttributes.ReadOnly);
        }

        public void WriteAllText(string text)
        {
            Parent.CreateDirectory();

            //Read only generated files
            if (Exists())
                FileInfo.Attributes = FileAttributes.Normal;

            File.WriteAllText(PathFull, text, new UTF8Encoding(false, true));
        }

        public void CopyTo(FilePath target)
        {
            if ((FileInfo.Attributes & FileAttributes.Encrypted) != 0)
                FileInfo.Attributes &= ~FileAttributes.Encrypted;

            File.Copy(PathFull, target.PathFull, overwrite: true);
        }

        #endregion File content operations

    }
}
