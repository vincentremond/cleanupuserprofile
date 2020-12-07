using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckHiddenActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public CheckHiddenActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "CheckHidden";

        public IAction GetAction(object value) => new CheckHiddenAction(_fileSystemOperator, value as string);
    }
}
