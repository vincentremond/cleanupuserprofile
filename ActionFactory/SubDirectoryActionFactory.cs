using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.ActionFactory
{
    internal class SubDirectoryActionFactory : IActionFactory<DirectoryAction>
    {
        public string ActionName => "SubDirectory";

        public DirectoryAction GetAction(
            object value)
        {
            return new DirectoryAction(value as SubDirectory);
        }
    }
}