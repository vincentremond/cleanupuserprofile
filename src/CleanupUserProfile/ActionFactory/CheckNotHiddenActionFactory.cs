using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class CheckNotHiddenActionFactory : IActionFactory
    {
        private readonly IFileSystemOperator _fileSystemOperator;

        public CheckNotHiddenActionFactory(IFileSystemOperator fileSystemOperator)
        {
            _fileSystemOperator = fileSystemOperator;
        }

        public string ActionName => "CheckNotHidden";

        public IAction GetAction(object value) => new CheckNotHiddenAction(_fileSystemOperator, value as string);
    }
}
