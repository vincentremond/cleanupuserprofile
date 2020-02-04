using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class RemoveActionFactory : IActionFactory<RemoveAction>
    {
        public string ActionName => "Remove";

        public RemoveAction GetAction(
            object value)
        {
            return new RemoveAction(value as string);
        }
    }
}