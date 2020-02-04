using System.IO;
using System.Threading.Tasks;
using CleanupUserProfile.Config;
using CleanupUserProfile.Services.Contracts;
using YamlDotNet.Serialization;

namespace CleanupUserProfile.Services.Impl
{
    internal class ConfigFileReader : IConfigFileReader
    {
        public async Task<Root> ReadConfigFileAsync(
            string configFilePath)
        {
            var d = new Deserializer();
            var fileContents = await File.ReadAllTextAsync(configFilePath);
            return d.Deserialize<Root>(fileContents);
        }
    }
}