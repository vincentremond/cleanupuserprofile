using CleanupUserProfile.Actions;

namespace CleanupUserProfile.ActionFactory
{
    internal interface IActionFactory<out T> where T : IAction
    {
        string ActionName { get; }

        T GetAction(
            object value);
    }
}