using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilentOrbit.Disk;

namespace SilentOrbit.Sync
{
    /// <summary>
    /// Replicates source directory into a target directory.
    /// </summary>
    public class DirectoryReplicator
    {
        public static Stats Run(Job job)
        {
            var sync = new DirectoryReplicator(job);
            sync.Run();

            Console.ResetColor();
            return sync.stats;
        }

        readonly Job job;
        readonly Ignore ignore;
        readonly Queue queue;
        readonly Stats stats;

        public DirectoryReplicator(Job job)
        {
            this.job = job;
            this.ignore = job.Ignore;
            this.queue = job.Queue;
            this.stats = new Stats(job, queue);
        }

        void Run()
        {
            //Make sure we are replicating the right folders
            //Require matching files
            try
            {
                var sourceCheck = job.Source.CombineFile("sync.txt").ReadAllText();
                var targetCheck = job.Target.CombineFile("sync.txt").ReadAllText();

                if (sourceCheck.StartsWith("SyncID:") == false)
                    throw new Exception("sync.txt must start with \"SyncID:\"");
                if (sourceCheck != targetCheck)
                    throw new Exception("sync.txt does not match in source and target");
            }
            catch (FileNotFoundException fnf)
            {
                throw new Exception("Missing sync.txt in " + fnf.FileName);
            }

            try
            {
                stats.Start();

                ReplicateDirsRecursive(job.Source, job.Target);

                queue.Wait();
            }
            finally
            {
                stats.Stop();
            }
        }

        /// <summary>
        /// Copies differences in source into target.
        /// </summary>
        void ReplicateDirsRecursive(DirPath source, DirPath target)
        {
            //Files
            queue.Run(() => ReplicateFiles(source, target));

            //Directories
            var sourceList = source.GetDirectories();
            var targetDic = target.GetDirectories().ToDictionary((f) => f.Name);

            foreach (var s in sourceList)
            {
                var name = s.Name;

                if (ignore.TestDirectoryName(name))
                    continue;

                if (targetDic.TryGetValue(name, out var t))
                {
                    //Directory exists on both sides

                    //Remove, dirs left will be deleted
                    targetDic.Remove(name);

                    //Update recursively
                    queue.Run(() => ReplicateDirsRecursive(s, t));
                }
                else
                {
                    t = target + (s - source);

                    queue.Run(() =>
                    {
                        //New directory
                        t.CreateDirectory();
                        stats.UpdateNew(t);

                        CopyDirsRecursive(s, t);
                    });
                }
            }

            foreach (var t in targetDic.Values)
            {
                //Deleted directories
                queue.Run(async () => await DeleteDirsRecursive(t));
            }
        }

        /// <summary>
        /// Same as <see cref="ReplicateDirsRecursive(DirPath, DirPath)"/> but 
        /// sends all files without comparing since we know the target doesn't exist.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CopyDirsRecursive(DirPath source, DirPath target)
        {
            //Files
            queue.Run(() => CopyFiles(source, target));

            //Directories
            var sourceList = source.GetDirectories();

            foreach (var s in sourceList)
            {
                var name = s.Name;

                if (ignore.TestDirectoryName(name))
                    continue;

                var t = target + (s - source);

                //New directory
                t.CreateDirectory();
                stats.UpdateNew(t);

                queue.Run(() => CopyDirsRecursive(s, t));
            }
        }

        /// <summary>
        /// Same as <see cref="ReplicateDirsRecursive(DirPath, DirPath)"/> but 
        /// deletes all target files.
        /// 
        /// When run in parallel this is significantly faster than Directory.Delete(..., recursive: true);
        /// </summary>
        async Task DeleteDirsRecursive(DirPath target)
        {
            var all = new TaskAll(queue);

            //Directories
            var targetList = target.GetDirectories();
            foreach (var t in targetList)
            {
                all.Run(async () => await DeleteDirsRecursive(t));
            }

            //Files
            all.Add(DeleteFiles(target));

            await all.WhenAll();
            target.DeleteEmptyDir();
            stats.UpdateDel(target);
        }

        /// <summary>
        /// Copies files in this directory pair, no subdirectories are copied here.
        /// </summary>
        void ReplicateFiles(DirPath source, DirPath target)
        {
            //Files
            var sourceList = source.GetFiles();
            var targetDic = target.GetFiles().ToDictionary((f) => f.Name);

            foreach (var s in sourceList)
            {
                var name = s.Name;

                if (ignore.TestFilename(name))
                    continue;

                Action reportstats = null;

                if (targetDic.TryGetValue(name, out var t))
                {
                    //Files exist on both sides

                    //Remove, files left will be deleted
                    targetDic.Remove(name);

                    //Update?
                    if (CheckUpdate(s, t))
                    {
                        //Modified, update
                        reportstats = () => stats.UpdateMod(s);
                    }
                    else
                        continue;
                }
                else
                {
                    t = target + (s - source);

                    //New
                    reportstats = () => stats.UpdateNew(s);
                }

                //New and updated
                queue.Run(() =>
                {
                    s.CopyTo(t);
                    reportstats?.Invoke();
                });
            }

            foreach (var t in targetDic.Values)
            {
                //Deleted
                queue.Run(() =>
                {
                    t.DeleteFile();
                    stats.UpdateDel(t);
                });
            }
        }

        /// <summary>
        /// Similar to <see cref="ReplicateFiles(DirPath, DirPath)"/> but
        /// sends all files without comparing since we know the target doesn't exist.
        /// </summary>
        void CopyFiles(DirPath source, DirPath target)
        {
            //Files
            var sourceList = source.GetFiles();

            foreach (var s in sourceList)
            {
                var name = s.Name;

                if (ignore.TestFilename(name))
                    continue;

                var t = target + (s - source);

                //New and updated
                queue.Run(() =>
                {
                    s.CopyTo(t);
                    stats.UpdateNew(s);
                });
            }
        }

        /// <summary>
        /// Similar to <see cref="ReplicateFiles(DirPath, DirPath)"/> but
        /// deletes all target files
        /// </summary>
        Task DeleteFiles(DirPath target)
        {
            var all = queue.WhenAll();

            //Delete Files
            var targetList = target.GetFiles();

            foreach (var t in targetList)
            {
                all.Run(() =>
                {
                    t.DeleteFile();
                    stats.UpdateDel(t);
                    return Task.CompletedTask;
                });
            }

            return all.WhenAll();
        }

        /// <summary>
        /// Check if the source is newer or have a different size than target.
        /// Return true if changes are detected and the file needs top be copied.
        /// 
        /// If attributes are different they will be updated here 
        /// but the return value will be false since no copy is needed.
        /// </summary>
        /// <returns></returns>
        bool CheckUpdate(FilePath sourceFile, FilePath targetFile)
        {
            var si = sourceFile.FileInfo;
            var ti = targetFile.FileInfo;

            if (si.Length != ti.Length)
                return true;

            if (si.LastWriteTimeUtc > ti.LastWriteTimeUtc)
                return true;

            if (job.SyncAttributes)
            {
                if (si.Attributes != ti.Attributes)
                {
                    targetFile.SetAttributes(si.Attributes);
                    stats.UpdateAtt(sourceFile);
                    return false; //nothing more needs to change
                }
            }

            //Else already in sync
            stats.UpdateNothing();
            return false;
        }
    }
}
