using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal interface IActionFactory
    {
        string ActionName { get; }

        IAction GetAction(object value);
    }
}
