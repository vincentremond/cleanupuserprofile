using System;
using System.IO;
using System.Linq;
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
                    var files = directory
                        .GetFiles("*", SearchOption.AllDirectories)
                        .ToList();
                    foreach (var f in files)
                    {
                        _fileSystemOperator.DeleteFile(f);
                        Console.WriteLine($" Removed : {f.FullName}");
                    }

                    _fileSystemOperator.DeleteDirectory(directory, true);
                    Console.WriteLine($" Removed : {directory.FullName}");

                    break;
                }
                default: throw new ApplicationException();
            }
        }
    }
}