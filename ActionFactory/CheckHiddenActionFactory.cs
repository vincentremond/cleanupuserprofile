using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckHiddenActionFactory : IActionFactory
    {
        public string ActionName => "CheckHidden";

        public IAction GetAction(
            object value)
        {
            return new CheckHiddenAction(value as string);
        }
    }
}