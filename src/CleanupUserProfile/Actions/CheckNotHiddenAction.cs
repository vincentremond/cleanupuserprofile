using System;
using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckNotHiddenAction : BaseAction
    {
        public CheckNotHiddenAction(
            string pattern) : base(pattern)
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