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

        public override void Execute(
            FileSystemInfo fileSystemInfo)
        {
            switch (fileSystemInfo)
            {
                case FileInfo file:
                {
                    file.Delete();
                    break;
                }
                case DirectoryInfo directory:
                {
                    directory
                        .GetFiles("*", SearchOption.AllDirectories)
                        .ToList()
                        .ForEach(f => f.Attributes = FileAttributes.Normal);
                    directory
                        .GetFiles("*", SearchOption.AllDirectories)
                        .ToList()
                        .ForEach(f => f.Delete());
                    directory.Delete(true);
                    break;
                }
                default: throw new ApplicationException();
            }
        }
    }
}