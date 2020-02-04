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
    }

    internal class FileRule : GenericRule
    {
    }

    internal class FolderRule : GenericRule
    {
        public string CheckEmptyFolder { get; set; }
        public string CheckEmptyFolderAndHide { get; set; }
        public SubDirectory SubDirectory { get; set; }
    }
}