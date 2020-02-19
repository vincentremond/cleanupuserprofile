using System;
using System.IO;
using System.Linq;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyDirectoryAction : BaseDirectoryAction
    {
        public CheckEmptyDirectoryAction(
            string pattern) : base(pattern)
        {
        }

        protected override void Execute(
            DirectoryInfo directory)
        {
            var files = directory
                .GetFiles()
                .Cast<FileSystemInfo>()
                .Union(directory.GetDirectories())
                .Where(fsi => !IsDesktopIni(fsi))
                .ToList();

            foreach (var file in files)
            {
                Console.WriteLine($" Remove me : {file.FullName}");
            }
        }
    }
}