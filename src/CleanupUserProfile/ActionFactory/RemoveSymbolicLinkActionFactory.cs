using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class RemoveSymbolicLinkActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public RemoveSymbolicLinkActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "RemoveSymbolicLink";

        public IAction GetAction(object value) => new RemoveSymbolicLinkAction(_fileSystemOperator, value as string);
    }
}
