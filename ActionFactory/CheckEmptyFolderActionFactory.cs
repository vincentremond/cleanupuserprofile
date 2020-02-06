using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyDirectoryActionFactory : IActionFactory
    {
        public string ActionName => "CheckEmptyDirectory";

        public IAction GetAction(
            object value)
        {
            return new CheckEmptyDirectoryAction(value as string);
        }
    }
}