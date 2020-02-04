using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class IgnoreActionFactory : IActionFactory<IgnoreAction>
    {
        public string ActionName => "Ignore";

        public IgnoreAction GetAction(
            object value)
        {
            return new IgnoreAction(value as string);
        }
    }
}