using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyFolderActionFactory : IActionFactory
    {
        public string ActionName => "CheckEmptyFolder";

        public IAction GetAction(
            object value)
        {
            return new CheckEmptyFolderAction(value as string);
        }
    }
}