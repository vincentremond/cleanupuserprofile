using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IActionConverter
    {
        DirectoryAction GetDirectoryAction(FileRule[] configFiles, DirectoryRule[] configDirectories, string directoryPattern, string selfActionName);
    }
}