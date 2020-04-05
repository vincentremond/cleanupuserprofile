using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class CheckHiddenAction : BaseAction
    {
        public CheckHiddenAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        public override void Execute(
            FileSystemInfo file)
        {
            if (SetVisibility(file, Hide))
            {
                Console.WriteLine($" Hidden : {file.FullName}");
            }
        }
    }
}