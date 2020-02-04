using System.Threading.Tasks;
using CleanupUserProfile.Actions;
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

        public async Task CleanupAsync(
            string configFilePath)
        {
            var userProfile = _pathLocator.GetUserProfile();
            var config = await _configFileReader.ReadConfigFileAsync(configFilePath);

            var filesActions = _actionConverter.Convert(config.Files);
            var foldersActions = _actionConverter.Convert(config.Folders);
            var cleanup = new DirectoryAction(filesActions, foldersActions);
            cleanup.Execute(userProfile);
        }
    }
}