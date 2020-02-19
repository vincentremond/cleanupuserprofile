using System.IO;

namespace CleanupUserProfile.Actions
{
    internal interface IAction
    {
        bool IsMatch(FileSystemInfo fileInfo);

        void Execute(FileSystemInfo file);
    }
}