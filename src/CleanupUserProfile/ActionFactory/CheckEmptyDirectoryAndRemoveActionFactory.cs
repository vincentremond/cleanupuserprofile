using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyDirectoryAndRemoveActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public CheckEmptyDirectoryAndRemoveActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "CheckEmptyDirectoryAndRemove";
 
        public IAction GetAction(object value) => new CheckEmptyDirectoryAndRemoveAction(_fileSystemOperator, value as string);
    }
}
