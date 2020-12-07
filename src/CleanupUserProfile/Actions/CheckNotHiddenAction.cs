using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class CheckNotHiddenAction : BaseAction
    {
        public CheckNotHiddenAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        public override void Execute(
            FileSystemInfo file)
        {
            if (SetVisibility(file, Show))
            {
                Console.WriteLine($" Shown : {file.FullName}");
            }
        }
    }
}
