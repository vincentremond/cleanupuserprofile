using System;

namespace CleanupUserProfile.Config
{
    [Serializable]
    internal class Root
    {
        public FileRule[] Files { get; set; }
        public FolderRule[] Folders { get; set; }
    }

    [Serializable]
    internal class SubDirectory : Root
    {
        public string Name { get; set; }
    }
}