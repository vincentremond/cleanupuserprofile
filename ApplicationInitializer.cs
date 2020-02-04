using CleanupUserProfile.Services;
using Microsoft.Extensions.DependencyInjection;

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

            return serviceProvider.BuildServiceProvider();
        }
    }
}