using System;
using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckHiddenAction : BaseAction
    {
        public CheckHiddenAction(
            string pattern) : base(pattern)
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