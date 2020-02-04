using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckNotHiddenActionFactory : IActionFactory<CheckNotHiddenAction>
    {
        public string ActionName => "CheckNotHidden";

        public CheckNotHiddenAction GetAction(
            object value)
        {
            return new CheckNotHiddenAction(value as string);
        }
    }
}