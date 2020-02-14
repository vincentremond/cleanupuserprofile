using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class RemoveActionFactory : IActionFactory
    {
        public string ActionName => "Remove";

        public IAction GetAction(
            object value)
        {
            return new RemoveAction(value as string);
        }
    }
}