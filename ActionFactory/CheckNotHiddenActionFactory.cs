using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckNotHiddenActionFactory : IActionFactory
    {
        public string ActionName => "CheckNotHidden";

        public IAction GetAction(
            object value)
        {
            return new CheckNotHiddenAction(value as string);
        }
    }
}