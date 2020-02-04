using System.IO;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Actions
{
    internal class DirectoryAction : BaseAction
    {
        public DirectoryAction(
            Root directory) : base(null)
        {
            
        }
        
        public DirectoryAction(
            SubDirectory directory) : base(directory.Name)
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