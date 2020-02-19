using System;
using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyDirectoryAndHideAction : CheckEmptyDirectoryAction
    {
        public CheckEmptyDirectoryAndHideAction(
            string pattern) : base(pattern)
        {
        }

        protected override void Execute(
            DirectoryInfo directory)
        {
            base.Execute(directory);

            if (SetVisibility(directory, Hide))
            {
                Console.WriteLine($" Hidden : {directory.FullName}");
            }
        }
    }
}