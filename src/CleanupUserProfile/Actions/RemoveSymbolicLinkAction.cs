using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class RemoveSymbolicLinkAction : BaseDirectoryAction
    {
        public RemoveSymbolicLinkAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        protected override void Execute(
            DirectoryInfo directory)
        {
            _fileSystemOperator.DeleteDirectory(directory, false);
            Console.WriteLine($" Removed : {directory.FullName}");
        }
    }
}