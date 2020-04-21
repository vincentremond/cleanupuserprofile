using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal class DirectoryAction : BaseAction
    {
        private readonly IAction _selfAction;
        private readonly IEnumerable<IAction> _filesActions;
        private readonly IEnumerable<IAction> _directoriesActions;

        public DirectoryAction(
            IFileSystemOperator fileSystemOperator,
            IAction selfAction,
            IEnumerable<IAction> filesActions,
            IEnumerable<IAction> directoriesActions,
            string pattern) : base(fileSystemOperator, pattern)
        {
            _selfAction = selfAction;
            _filesActions = filesActions;
            _directoriesActions = directoriesActions;
        }

        public override void Execute(
            FileSystemInfo fileSystemInfo)
        {
            if (!fileSystemInfo.Exists)
            {
                return;
            }

            if (!(fileSystemInfo is DirectoryInfo directoryInfo))
            {
                return;
            }

            _selfAction?.Execute(fileSystemInfo);
            PerformActions(_filesActions, directoryInfo.GetFiles());
            PerformActions(_directoriesActions, directoryInfo.GetDirectories());
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