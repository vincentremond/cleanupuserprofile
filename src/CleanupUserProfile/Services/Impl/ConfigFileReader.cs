using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CleanupUserProfile.Services.Contracts;
using YamlDotNet.Serialization;
using Directory = CleanupUserProfile.Config.Directory;

namespace CleanupUserProfile.Services.Impl
{
    internal class ConfigFileReader : IConfigFileReader
    {
        public async Task<IEnumerable<Directory>> ReadConfigFileAsync(string configFilePath)
        {
            var d = new Deserializer();
            var fileContents = await File.ReadAllTextAsync(configFilePath);
            return d.Deserialize<IEnumerable<Directory>>(fileContents);
        }
    }
}