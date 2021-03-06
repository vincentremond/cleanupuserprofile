﻿using System;
using System.Collections.Generic;
using System.Linq;
using CleanupUserProfile.ActionFactory;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Services.Impl
{
    internal class ActionConverter : IActionConverter
    {
        private readonly IEnumerable<IActionFactory> _actionFactories;
        private readonly IFileSystemOperator _fileSystemOperator;

        public ActionConverter(
            IEnumerable<IActionFactory> actionFactories,
            IFileSystemOperator fileSystemOperator)
        {
            _actionFactories = actionFactories;
            _fileSystemOperator = fileSystemOperator;
        }

        public DirectoryAction GetDirectoryAction(FileRule[] configFiles, DirectoryRule[] configDirectories,
            string directoryName, string selfActionName)
        {
            var selfAction = !string.IsNullOrEmpty(selfActionName) ? ConvertSingle(selfActionName, directoryName) : null;
            var filesActions = Convert(configFiles);
            filesActions.Add(new IgnoreAction(_fileSystemOperator, "desktop.ini"));
            var directoriesActions = Convert(configDirectories);
            return new DirectoryAction(_fileSystemOperator, selfAction, filesActions, directoriesActions, directoryName);
        }

        public IList<IAction> Convert(
            IEnumerable<GenericRule> configFiles)
        {
            if (configFiles == null)
            {
                return new List<IAction>(0);
            }

            return configFiles
                .Select(ConvertSingle)
                .ToList();
        }

        private IAction ConvertSingle(
            GenericRule arg)
        {
            var (name, value) = GetActionNameAndValue(arg);
            return ConvertSingle(name, value);
        }

        private IAction ConvertSingle(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var actionFactory = _actionFactories.SingleOrDefault(a => a.ActionName == name);
            if (actionFactory == null)
            {
                throw new Exception($"Failed to determine action for {name}.");
            }

            if (value is Directory directory)
            {
                return GetDirectoryAction(directory.Files, directory.Directories, directory.Name, directory.Self);
            }

            return actionFactory.GetAction(value);
        }

        private static (string Name, object Value) GetActionNameAndValue(
            GenericRule genericRule)
        {
            var props = genericRule.GetType().GetProperties();
            return (
                from p in props
                let v = p.GetValue(genericRule)
                where v != null
                select (p.Name, v)
            ).Single();
        }
    }
}
