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
            // TODO VRM
            throw new System.NotImplementedException();
        }
    }
}