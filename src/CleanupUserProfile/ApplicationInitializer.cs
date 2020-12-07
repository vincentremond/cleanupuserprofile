using CleanupUserProfile.ActionFactory;
using CleanupUserProfile.Services.Contracts;
using CleanupUserProfile.Services.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanupUserProfile
{
    internal static class ApplicationInitializer
    {
        public static ServiceProvider Init(bool simulate)
        {
            var serviceProvider = new ServiceCollection();

            serviceProvider.AddTransient<IUserProfileCleaner, UserProfileCleaner>();
            serviceProvider.AddTransient<IConfigFileReader, ConfigFileReader>();
            serviceProvider.AddTransient<IPathLocator, EnvironmentPathLocator>();
            serviceProvider.AddTransient<IActionConverter, ActionConverter>();

            serviceProvider.AddTransient<IActionFactory, CheckHiddenActionFactory>();
            serviceProvider.AddTransient<IActionFactory, CheckNotHiddenActionFactory>();
            serviceProvider.AddTransient<IActionFactory, CheckEmptyDirectoryActionFactory>();
            serviceProvider.AddTransient<IActionFactory, CheckEmptyDirectoryAndHideActionFactory>();
            serviceProvider.AddTransient<IActionFactory, CheckEmptyDirectoryAndRemoveActionFactory>();
            serviceProvider.AddTransient<IActionFactory, IgnoreActionFactory>();
            serviceProvider.AddTransient<IActionFactory, RemoveActionFactory>();
            serviceProvider.AddTransient<IActionFactory, DirectoryActionFactory>();
            serviceProvider.AddTransient<IActionFactory, RemoveSymbolicLinkActionFactory>();
            serviceProvider.AddTransient<IActionFactory, QuietRemoveActionFactory>();

            if (simulate)
            {
                serviceProvider.AddTransient<IFileSystemOperator, SimulationFileSystemOperator>();
            }
            else
            {
                serviceProvider.AddTransient<IFileSystemOperator, RealFileSystemOperator>();
            }

            serviceProvider.AddLogging(builder => builder.AddConsole());

            return serviceProvider.BuildServiceProvider();
        }
    }
}
