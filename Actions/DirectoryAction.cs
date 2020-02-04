using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanupUserProfile.Actions
{
    internal class DirectoryAction : BaseAction
    {
        private readonly IEnumerable<IAction> _filesActions;
        private readonly IEnumerable<IAction> _foldersActions;

        public DirectoryAction(
            IEnumerable<IAction> filesActions,
            IEnumerable<IAction> foldersActions,
            string pattern = null) : base(pattern)
        {
            _filesActions = filesActions;
            _foldersActions = foldersActions;
        }

        public override void Execute(
            FileSystemInfo fileSystemInfo)
        {
            if (!fileSystemInfo.Exists) return;

            if (!(fileSystemInfo is DirectoryInfo folderInfo)) return;

            PerformActions(_filesActions, folderInfo.GetFiles());
            PerformActions(_foldersActions, folderInfo.GetDirectories());
        }

        private static void PerformActions(
            IEnumerable<IAction> filesActions,
            IEnumerable<FileSystemInfo> fileSystemInfos)
        {
            foreach (var fileSystemInfo in fileSystemInfos)
            {
                var action = filesActions.FirstOrDefault(r => r.IsMatch(fileSystemInfo));
                if (action == null)
                    Console.WriteLine($" What to do ? {fileSystemInfo.GetType().Name} > {fileSystemInfo.FullName}");
                else
                    action.Execute(fileSystemInfo);
            }
        }
    }
}