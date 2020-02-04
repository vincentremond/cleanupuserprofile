using System;
using System.Threading.Tasks;

namespace CleanupUserProfile.Services
{
    internal class UserProfileCleaner : IUserProfileCleaner
    {
        private readonly IConfigFileReader _configFileReader;
        private readonly IPathLocator _pathLocator;

        public UserProfileCleaner(
            IConfigFileReader configFileReader,
            IPathLocator pathLocator)
        {
            _configFileReader = configFileReader;
            _pathLocator = pathLocator;
        }

        public async Task CleanupAsync(
            string configFilePath)
        {
            // TODO VRM
            var userProfile = _pathLocator.GetUserProfile();
            var config = await _configFileReader.ReadConfigFileAsync(configFilePath);
            Console.WriteLine(config);
        }
    }
}