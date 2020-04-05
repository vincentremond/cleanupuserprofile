using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyDirectoryAndHideAction : CheckEmptyDirectoryAction
    {
        public CheckEmptyDirectoryAndHideAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        protected override void Execute(DirectoryInfo directory)
        {
            base.Execute(directory);

            if (SetVisibility(directory, Hide))
            {
                Console.WriteLine($" Hidden : {directory.FullName}");
            }
        }
    }
}