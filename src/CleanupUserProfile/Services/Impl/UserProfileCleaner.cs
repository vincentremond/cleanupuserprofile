using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Services.Contracts;
using Directory = CleanupUserProfile.Config.Directory;

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
            var directoriesInfo = GetDirectoriesInfo(config);
            var directoriesActions = GetDirectoriesActions(directoriesInfo);
            PerformCleanup(directoriesActions);
        }

        private static void PerformCleanup(List<(DirectoryInfo, DirectoryAction)> directoriesActions)
        {
            foreach (var (directoryInfo, directoryCleanupAction) in directoriesActions)
            {
                if (!directoryInfo.Exists)
                {
                    continue;
                }

                directoryCleanupAction.Execute(directoryInfo);
            }
        }

        private List<(DirectoryInfo, DirectoryAction)> GetDirectoriesActions(
            IEnumerable<(DirectoryInfo, Directory)> directoriesInfo)
        {
            var result = new List<(DirectoryInfo, DirectoryAction)>();
            foreach (var (directoryInfo, directory) in directoriesInfo)
            {
                var directoryCleanupActions =
                    _actionConverter.GetDirectoryAction(
                        directory.Files,
                        directory.Directories,
                        null,
                        null
                    );
                result.Add((directoryInfo, directoryCleanupActions));
            }

            return result;
        }

        private IEnumerable<(DirectoryInfo, Directory)> GetDirectoriesInfo(IEnumerable<Directory> config)
        {
            var result = new List<(DirectoryInfo, Directory)>();
            foreach (var directory in config)
            {
                var info = _pathLocator.Locate(directory.Name);
                result.Add((info, directory));
            }

            return result;
        }
    }
}
