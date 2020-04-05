using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyDirectoryAndHideActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public CheckEmptyDirectoryAndHideActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "CheckEmptyDirectoryAndHide";

        public IAction GetAction(object value)
        {
            return new CheckEmptyDirectoryAndHideAction(_fileSystemOperator, value as string);
        }
    }
}