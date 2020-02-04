using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyFolderAction : BaseAction
    {
        public CheckEmptyFolderAction(
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