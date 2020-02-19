using System.Threading.Tasks;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IConfigFileReader
    {
        Task<Root> ReadConfigFileAsync(string configFilePath);
    }
}