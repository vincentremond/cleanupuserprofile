using System.IO;

namespace CleanupUserProfile.Services
{
    internal interface IPathLocator
    {
        DirectoryInfo GetUserProfile();
    }
}