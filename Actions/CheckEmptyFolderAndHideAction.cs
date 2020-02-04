using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyFolderAndHideAction : BaseAction
    {
        public CheckEmptyFolderAndHideAction(
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