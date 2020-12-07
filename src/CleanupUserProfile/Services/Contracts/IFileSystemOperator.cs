using System.IO;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IFileSystemOperator
    {
        void DeleteDirectory(DirectoryInfo directory, bool recursive);
        void SetFileAttributes(string fullName, FileAttributes newAttributes);
        void DeleteFile(FileInfo file);
    }
}
