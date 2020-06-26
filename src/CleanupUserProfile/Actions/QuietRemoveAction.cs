using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class QuietRemoveAction : RemoveAction
    {
        protected override bool IsVerbose => false;

        public QuietRemoveAction(IFileSystemOperator fileSystemOperator, string pattern) : base(fileSystemOperator, pattern)
        {
        }
    }
}