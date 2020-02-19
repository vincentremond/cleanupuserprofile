using System.Threading.Tasks;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class UserProfileCleaner : IUserProfileCleaner
    {
        private readonly IActionConverter _actionConverter;
        private readonly IConfigFileReader _configFileReader;
        private readonly IPathLocator _pathLocator;

        public UserProfileCleaner(
            IConfigFileReader configFileReader,
            IPathLocator pathLocator,
            IActionConverter actionConverter)
        {
            _configFileReader = configFileReader;
            _pathLocator = pathLocator;
            _actionConverter = actionConverter;
        }

        public async Task CleanupAsync(string configFilePath)
        {
            var config = await _configFileReader.ReadConfigFileAsync(configFilePath);
            var directoryCleanupAction = _actionConverter.GetDirectoryAction(config.Files, config.Directories);
            var userProfile = _pathLocator.GetUserProfile();
            directoryCleanupAction.Execute(userProfile);
        }
    }
}