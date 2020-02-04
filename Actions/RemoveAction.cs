using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class RemoveAction : BaseAction
    {
        public RemoveAction(
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