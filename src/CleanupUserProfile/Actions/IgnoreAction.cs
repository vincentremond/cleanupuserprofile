using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class IgnoreAction : BaseAction
    {
        public IgnoreAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        public override void Execute(FileSystemInfo file)
        {
        }
    }
}
