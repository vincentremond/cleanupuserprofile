using System.IO;

namespace CleanupUserProfile.Actions
{
    internal abstract class BaseDirectoryAction : BaseAction
    {
        protected BaseDirectoryAction(
            string pattern) : base(pattern)
        {
        }

        public override void Execute(
            FileSystemInfo file)
        {
            Execute(file as DirectoryInfo);
        }

        protected abstract void Execute(
            DirectoryInfo file);
    }
}