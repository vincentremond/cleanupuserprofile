using System.Collections.Generic;
using System.Threading.Tasks;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IConfigFileReader
    {
        Task<IEnumerable<Directory>> ReadConfigFileAsync(string configFilePath);
    }
}