using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyFolderActionFactory : IActionFactory<CheckEmptyFolderAction>
    {
        public string ActionName => "CheckEmptyFolder";

        public CheckEmptyFolderAction GetAction(
            object value)
        {
            return new CheckEmptyFolderAction(value as string);
        }
    }
}