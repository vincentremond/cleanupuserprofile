using System;

namespace CleanupUserProfile.Config
{
    [Serializable]
    internal abstract class GenericRule
    {
        public string CheckHidden { get; set; }
        public string CheckNotHidden { get; set; }
        public string Ignore { get; set; }
        public string Remove { get; set; }
        public string QuietRemove { get; set; }
    }

    [Serializable]
    internal class FileRule : GenericRule
    {
    }

    [Serializable]
    internal class DirectoryRule : GenericRule
    {
        public string CheckEmptyDirectory { get; set; }
        public string CheckEmptyDirectoryAndHide { get; set; }
        public string CheckEmptyDirectoryAndRemove { get; set; }
        public string RemoveSymbolicLink { get; set; }
        public Directory Directory { get; set; }
    }
}
