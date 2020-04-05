using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class RemoveActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public RemoveActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "Remove";

        public IAction GetAction(object value)
        {
            return new RemoveAction(_fileSystemOperator, value as string);
        }
    }
}