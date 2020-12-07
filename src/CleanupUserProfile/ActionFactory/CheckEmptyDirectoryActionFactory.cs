using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckEmptyDirectoryActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public CheckEmptyDirectoryActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "CheckEmptyDirectory";

        public IAction GetAction(object value) => new CheckEmptyDirectoryAction(_fileSystemOperator, value as string);
    }
}
