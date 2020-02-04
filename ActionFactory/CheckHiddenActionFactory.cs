using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckHiddenActionFactory : IActionFactory<CheckHiddenAction>
    {
        public string ActionName => "CheckHidden";

        public CheckHiddenAction GetAction(
            object value)
        {
            return new CheckHiddenAction(value as string);
        }
    }
}