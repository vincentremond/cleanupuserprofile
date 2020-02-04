using CleanupUserProfile.ActionFactory;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanupUserProfile
{
    internal static class ApplicationInitializer
    {
        public static ServiceProvider Init()
        {
            var serviceProvider = new ServiceCollection();

            serviceProvider.AddTransient<IUserProfileCleaner, UserProfileCleaner>();
            serviceProvider.AddTransient<IConfigFileReader, ConfigFileReader>();
            serviceProvider.AddTransient<IPathLocator, EnvironmentPathLocator>();
            serviceProvider.AddTransient<IActionConverter, ActionConverter>();
            
            serviceProvider.AddTransient<IActionFactory<CheckHiddenAction>, CheckHiddenActionFactory>();
            serviceProvider.AddTransient<IActionFactory<CheckNotHiddenAction>, CheckNotHiddenActionFactory>();
            serviceProvider.AddTransient<IActionFactory<CheckEmptyFolderAction>, CheckEmptyFolderActionFactory>();
            serviceProvider.AddTransient<IActionFactory<CheckEmptyFolderAndHideAction>, CheckEmptyFolderAndHideActionFactory>();
            serviceProvider.AddTransient<IActionFactory<IgnoreAction>, IgnoreActionFactory>();
            serviceProvider.AddTransient<IActionFactory<RemoveAction>, RemoveActionFactory>();
            serviceProvider.AddTransient<IActionFactory<DirectoryAction>, SubDirectoryActionFactory>();

            serviceProvider.AddLogging(builder => builder.AddConsole());

            return serviceProvider.BuildServiceProvider();
        }
    }
}