using System;
using System.IO;
using System.Linq;

namespace CleanupUserProfile.Actions
{
    internal class RemoveAction : BaseAction
    {
        public RemoveAction(
            string pattern) : base(pattern)
        {
        }

        public override void Execute(FileSystemInfo fileSystemInfo)
        {
            switch (fileSystemInfo)
            {
                case FileInfo file:
                {
                    file.Delete();
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
                        f.Attributes = FileAttributes.Normal;
                        f.Delete();
                        Console.WriteLine($" Removed : {f.FullName}");
                    }

                    directory.Delete(true);
                    Console.WriteLine($" Removed : {directory.FullName}");

                    break;
                }
                default: throw new ApplicationException();
            }
        }
    }
}