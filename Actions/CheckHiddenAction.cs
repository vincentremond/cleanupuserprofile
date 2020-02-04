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
            // TODO VRM
            throw new System.NotImplementedException();
        }
    }
}