using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class RemoveSymbolicLinkActionFactory : IActionFactory
    {
        public string ActionName => "RemoveSymbolicLink";

        public IAction GetAction(object value)
        {
            return new RemoveSymbolicLinkAction(value as string);
        }
    }
}