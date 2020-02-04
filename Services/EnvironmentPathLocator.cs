using System;

namespace CleanupUserProfile.Services
{
    internal class EnvironmentPathLocator : IPathLocator
    {
        public string GetUserProfile() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}