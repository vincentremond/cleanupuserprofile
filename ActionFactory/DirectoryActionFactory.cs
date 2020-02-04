using System;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.ActionFactory
{
    internal class DirectoryActionFactory : IActionFactory
    {
        private readonly IActionConverter _actionConverter;

        public DirectoryActionFactory(
            IActionConverter actionConverter)
        {
            _actionConverter = actionConverter;
        }

        public string ActionName => "SubDirectory";

        public IAction GetAction(
            object value)
        {
            if (!(value is SubDirectory subDirectory)) throw new ArgumentException($"Failed to cast as {nameof(SubDirectory)}", nameof(value));

            var filesActions = _actionConverter.Convert(subDirectory.Files);
            var foldersActions = _actionConverter.Convert(subDirectory.Folders);

            return new DirectoryAction(filesActions, foldersActions, subDirectory.Name);
        }
    }
}