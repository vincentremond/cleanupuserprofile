using System;
using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal class DirectoryActionFactory : IActionFactory
    {
        public string ActionName => "SubDirectory";

        public IAction GetAction(
            object value)
        {
            throw new InvalidOperationException();
        }
    }
}