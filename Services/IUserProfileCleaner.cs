using System.Threading.Tasks;

namespace CleanupUserProfile.Services
{
    internal interface IUserProfileCleaner
    {
        Task CleanupAsync(
            string configFilePath);
    }
}