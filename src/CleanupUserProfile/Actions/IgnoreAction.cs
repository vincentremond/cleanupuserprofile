using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class IgnoreAction : BaseAction
    {
        public IgnoreAction(string pattern) : base(pattern)
        {
        }

        public override void Execute(FileSystemInfo file)
        {
        }
    }
}