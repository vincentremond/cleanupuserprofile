using System;

namespace CleanupUserProfile.Config
{
    [Serializable]
    internal class Directory
    {
        public string Name { get; set; }
        public FileRule[] Files { get; set; }
        public DirectoryRule[] Directories { get; set; }
    }
}