using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class IgnoreActionFactory : IActionFactory
    {
        public string ActionName => "Ignore";

        public IAction GetAction(
            object value)
        {
            return new IgnoreAction(value as string);
        }
    }
}