using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class QuietRemoveActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public QuietRemoveActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "QuietRemove";

        public IAction GetAction(object value)
        {
            return new QuietRemoveAction(_fileSystemOperator, value as string);
        }
    }
}