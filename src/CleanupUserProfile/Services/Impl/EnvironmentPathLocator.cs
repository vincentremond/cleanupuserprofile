using System;
using System.IO;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class EnvironmentPathLocator : IPathLocator
    {
        public DirectoryInfo Locate(string directoryName)
        {
            if (directoryName == "~")
            {
                return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            }

            return new DirectoryInfo(directoryName);
        }
    }
}