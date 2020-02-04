using System.IO;

namespace CleanupUserProfile.Actions
{
    internal class CheckEmptyFolderAndHideAction : CheckEmptyFolderAction
    {
        public CheckEmptyFolderAndHideAction(
            string pattern) : base(pattern)
        {
        }

        protected override void Execute(
            DirectoryInfo directory)
        {
            base.Execute(directory);
            SetVisibility(directory, Hide);
        }
    }
}