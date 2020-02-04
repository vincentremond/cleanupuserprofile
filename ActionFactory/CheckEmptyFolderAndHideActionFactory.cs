using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyFolderAndHideActionFactory : IActionFactory<CheckEmptyFolderAndHideAction>
    {
        public string ActionName => "CheckEmptyFolderAndHide";

        public CheckEmptyFolderAndHideAction GetAction(
            object value)
        {
            return new CheckEmptyFolderAndHideAction(value as string);
        }
    }
}