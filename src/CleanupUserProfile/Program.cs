using System.Threading.Tasks;
using CleanupUserProfile.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace CleanupUserProfile
{
    // ReSharper disable once UnusedType.Global
    internal class Program
    {
        // ReSharper disable once UnusedMember.Local
        private static async Task Main(string configFile = "sample.yml")
        {
            var serviceProvider = ApplicationInitializer.Init();
            var userProfileCleaner = serviceProvider.GetRequiredService<IUserProfileCleaner>();
            await userProfileCleaner.CleanupAsync(configFile);
        }
    }
}