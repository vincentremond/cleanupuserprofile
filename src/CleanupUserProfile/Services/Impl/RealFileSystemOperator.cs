using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class RealFileSystemOperator : IFileSystemOperator
    {
        public void SetFileAttributes(string fullName, FileAttributes newAttributes)
        {
            File.SetAttributes(fullName, newAttributes);
        }

        public void DeleteFile(FileInfo file)
        {
            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }

        public void DeleteDirectory(DirectoryInfo directory, bool recursive)
        {
            directory.Delete(recursive);
        }
    }
}