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
            SetVisibility(file, Show);
        }
    }
}