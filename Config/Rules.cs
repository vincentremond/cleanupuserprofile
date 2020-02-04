using System;
using CleanupUserProfile.Actions;

namespace CleanupUserProfile.Config
{
    [Serializable]
    internal abstract class GenericRule
    {
        public string CheckHidden
        {
            set => SetAction(new CheckHiddenAction(value));
        }

        public string CheckNotHidden
        {
            set => SetAction(new CheckNotHiddenAction(value));
        }

        public string Ignore
        {
            set => SetAction(new IgnoreAction(value));
        }

        public string Remove
        {
            set => SetAction(new RemoveAction(value));
        }

        internal BaseAction Action { get; set; }

        internal void SetAction(
            BaseAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Action != null)
            {
                throw new Exception("A rule can only have one action");
            }

            Action = action;
        }
    }

    internal class FileRule : GenericRule
    {
    }

    internal class FolderRule : GenericRule
    {
        public string CheckEmptyFolder
        {
            set => SetAction(new CheckEmptyFolderAction(value));
        }

        public string CheckEmptyFolderAndHide
        {
            set => SetAction(new CheckEmptyFolderAndHideAction(value));
        }

        public SubDirectory SubDirectory
        {
            set => SetAction(new SubDirectoryAction(value));
        }
    }
}