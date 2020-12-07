using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyDirectoryAndRemoveAction : CheckEmptyDirectoryAction
    {
        public CheckEmptyDirectoryAndRemoveAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        protected override void Execute(DirectoryInfo directory)
        {
            if (!CheckEmpty(directory))
            {
                return;
            }

            RemoveDirectory(directory);
        }
    }
}
