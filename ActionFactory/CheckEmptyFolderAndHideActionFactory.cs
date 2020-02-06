using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyDirectoryAndHideActionFactory : IActionFactory
    {
        public string ActionName => "CheckEmptyDirectoryAndHide";

        public IAction GetAction(
            object value)
        {
            return new CheckEmptyDirectoryAndHideAction(value as string);
        }
    }
}