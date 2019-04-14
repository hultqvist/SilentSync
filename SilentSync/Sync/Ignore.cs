using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilentOrbit.Sync
{
    public class Ignore
    {
        const RegexOptions ignoreOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        readonly List<Regex> ignoreFileNames = new List<Regex>();
        readonly List<Regex> ignoreDirNames = new List<Regex>();

        /// <summary>
        /// Files to be ignored/removed from target.
        /// </summary>
        /// <param name="regex"></param>
        public void IgnoreFileNameRegex(string regex)
        {
            ignoreFileNames.Add(new Regex("^" + regex + "$", ignoreOptions));
        }

        public void IgnoreFileName(string name)
        {
            var pattern = "^" + Regex.Escape(name) + "$";
            ignoreFileNames.Add(new Regex(pattern, ignoreOptions));
        }

        /// <summary>
        /// Files to be ignored/removed from target.
        /// </summary>
        /// <param name="regex"></param>
        public void IgnoreDirNameRegex(string regex)
        {
            ignoreDirNames.Add(new Regex("^" + regex + "$", ignoreOptions));
        }

        public void IgnoreDirName(string name)
        {
            var pattern = "^" + Regex.Escape(name) + "$";
            ignoreDirNames.Add(new Regex(pattern, ignoreOptions));
        }

        internal bool TestFilename(string name) => IsMatch(ignoreFileNames, name);
        internal bool TestDirectoryName(string name) => IsMatch(ignoreDirNames, name);

        static bool IsMatch(List<Regex> ignoreFileNames, string name)
        {
            foreach (var re in ignoreFileNames)
            {
                if (re.IsMatch(name))
                    return true;
            }

            return false;
        }
    }
}
