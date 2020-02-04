using System;
using System.IO;

namespace CleanupUserProfile.Services
{
    internal class EnvironmentPathLocator : IPathLocator
    {
        public DirectoryInfo GetUserProfile() => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }
}