using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class RemoveAction : BaseAction
    {
        public RemoveAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        public override void Execute(FileSystemInfo fileSystemInfo)
        {
            switch (fileSystemInfo)
            {
                case FileInfo file:
                {
                    _fileSystemOperator.DeleteFile(file);
                    Console.WriteLine($" Removed : {file.FullName}");

                    break;
                }
                case DirectoryInfo directory:
                {
                    RemoveDirectory(directory);

                    break;
                }
                default: throw new ApplicationException();
            }
        }
    }
}
