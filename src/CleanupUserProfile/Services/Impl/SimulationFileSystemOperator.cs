using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class SimulationFileSystemOperator : IFileSystemOperator
    {
        public void DeleteDirectory(DirectoryInfo directory, bool recursive)
        {
            // Console.Write($"Fake directory delete {directory.FullName} (recursive:{recursive})");
        }

        public void SetFileAttributes(string fullName, FileAttributes newAttributes)
        {
            // Console.Write($"Fake set file attributes {fullName} : {newAttributes:F}");
        }

        public void DeleteFile(FileInfo file)
        {
            // Console.Write($"Fake file delete {file.FullName}");
        }
    }
}
