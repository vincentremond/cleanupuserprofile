using System;
using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class RemoveSymbolicLinkAction : BaseDirectoryAction
    {
        public RemoveSymbolicLinkAction(string pattern) : base(pattern)
        {
        }

        protected override void Execute(
            DirectoryInfo directory)
        {
            directory.Delete(false);
            Console.WriteLine($" Removed : {directory.FullName}");
        }
    }
}