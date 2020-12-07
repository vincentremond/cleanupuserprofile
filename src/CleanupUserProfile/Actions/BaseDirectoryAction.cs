using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal abstract class BaseDirectoryAction : BaseAction
    {
        protected BaseDirectoryAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        public override void Execute(FileSystemInfo file) => Execute(file as DirectoryInfo);

        protected abstract void Execute(DirectoryInfo file);
    }
}
