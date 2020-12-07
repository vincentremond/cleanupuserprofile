using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class IgnoreActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public IgnoreActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "Ignore";

        public IAction GetAction(object value) => new IgnoreAction(_fileSystemOperator, value as string);
    }
}
