using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class QuietRemoveAction : RemoveAction
    {
        public QuietRemoveAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }

        protected override bool IsVerbose => false;
    }
}
