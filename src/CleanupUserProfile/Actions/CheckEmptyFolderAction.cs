using System;
using System.IO;
using System.Linq;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyDirectoryAction : BaseDirectoryAction
    {
        public CheckEmptyDirectoryAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        protected override void Execute(DirectoryInfo directory) => CheckEmpty(directory);

        protected static bool CheckEmpty(DirectoryInfo directory)
        {
            var filesAndDirectories = directory
                .GetFiles()
                .Cast<FileSystemInfo>()
                .Union(directory.GetDirectories())
                .Where(fsi => !IsDesktopIni(fsi))
                .ToList();

            foreach (var file in filesAndDirectories)
            {
                Console.WriteLine($" Please, remove me : {file.FullName}");
            }

            return !filesAndDirectories.Any();
        }
    }
}
