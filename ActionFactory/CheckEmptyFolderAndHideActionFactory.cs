using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyFolderAndHideActionFactory : IActionFactory
    {
        public string ActionName => "CheckEmptyFolderAndHide";

        public IAction GetAction(
            object value)
        {
            return new CheckEmptyFolderAndHideAction(value as string);
        }
    }
}