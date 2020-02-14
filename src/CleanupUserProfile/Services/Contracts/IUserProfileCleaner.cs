using System.Threading.Tasks;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IUserProfileCleaner
    {
        Task CleanupAsync(
            string configFilePath);
    }
}