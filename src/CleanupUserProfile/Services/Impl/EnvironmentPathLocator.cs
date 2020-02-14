using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class EnvironmentPathLocator : IPathLocator
    {
        public DirectoryInfo GetUserProfile()
        {
            return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
    }
}