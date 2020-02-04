using System.Threading.Tasks;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services
{
    internal interface IConfigFileReader
    {
        Task<Root> ReadConfigFileAsync(
            string configFilePath);
    }
}