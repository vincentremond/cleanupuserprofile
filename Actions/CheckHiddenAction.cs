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
            SetVisibility(file, Hide);
        }
    }
}