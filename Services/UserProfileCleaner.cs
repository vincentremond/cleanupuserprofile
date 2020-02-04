using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CleanupUserProfile.Actions;
using Microsoft.Extensions.Logging;

namespace CleanupUserProfile.Services
{
    internal class UserProfileCleaner : IUserProfileCleaner
    {
        private readonly IConfigFileReader _configFileReader;
        private readonly IPathLocator _pathLocator;
        private readonly ILogger _logger;
        private readonly IActionConverter _actionConverter;

        public UserProfileCleaner(
            IConfigFileReader configFileReader,
            IPathLocator pathLocator,
            ILogger<UserProfileCleaner> logger,
            IActionConverter actionConverter)
        {
            _configFileReader = configFileReader;
            _pathLocator = pathLocator;
            _logger = logger;
            _actionConverter = actionConverter;
        }

        public async Task CleanupAsync(
            string configFilePath)
        {
            // TODO VRM
            var userProfile = _pathLocator.GetUserProfile();
            var config = await _configFileReader.ReadConfigFileAsync(configFilePath);
            var filesActions = _actionConverter.Convert(config.Files);
            var foldersActions = _actionConverter.Convert(config.Folders);
            InnerCleanup(userProfile, filesActions, foldersActions);
        }

        private void InnerCleanup(
            DirectoryInfo folderInfo,
            IEnumerable<IAction> filesActions,
            IEnumerable<IAction> foldersActions)
        {
            if (!folderInfo.Exists)
            {
                return;
            }

            PerformActions(filesActions, folderInfo.GetFiles());
            PerformActions(foldersActions, folderInfo.GetDirectories());
        }

        private void PerformActions(
            IEnumerable<IAction> filesActions,
            IEnumerable<FileSystemInfo> fileSystemInfos)
        {
            foreach (var fileSystemInfo in fileSystemInfos)
            {
                var action = filesActions.FirstOrDefault(r => r.IsMatch(fileSystemInfo));
                if (action == null)
                {
                    _logger.LogWarning($" What to do ? {fileSystemInfo.GetType().Name} > {fileSystemInfo.FullName}");
                }
                else
                {
                    action.Execute(fileSystemInfo);
                }
            }
        }
    }
}