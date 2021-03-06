﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CleanupUserProfile.Services.Contracts;

namespace CleanupUserProfile.Actions
{
    internal abstract class BaseAction : IAction
    {
        protected readonly IFileSystemOperator _fileSystemOperator;
        private readonly Regex _pattern;

        protected BaseAction(IFileSystemOperator fileSystemOperator, string pattern)
        {
            _fileSystemOperator = fileSystemOperator;
            if (pattern != null)
            {
                _pattern = ToRegex(pattern);
            }
        }

        protected virtual bool IsVerbose => true;

        public bool IsMatch(FileSystemInfo fileInfo) => _pattern.IsMatch(fileInfo.Name);

        public abstract void Execute(FileSystemInfo file);

        private static Regex ToRegex(string rule)
        {
            const string regexPrefix = "REGEX:";
            var pattern = rule.StartsWith(regexPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? rule.Substring(regexPrefix.Length)
                : $"^{Regex.Escape(rule)}$";

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected static bool IsDesktopIni(FileSystemInfo file) => file is FileInfo x && x.Name.Equals("Desktop.ini", StringComparison.InvariantCultureIgnoreCase);

        protected bool SetVisibility<T>(
            T fileToModify,
            Func<FileAttributes, FileAttributes> modifyAction)
            where T : FileSystemInfo
        {
            var attributes = File.GetAttributes(fileToModify.FullName);
            var newAttributes = modifyAction(attributes);
            if (attributes == newAttributes)
            {
                return false;
            }

            _fileSystemOperator.SetFileAttributes(fileToModify.FullName, newAttributes);
            return true;
        }

        protected static FileAttributes Show(FileAttributes fileAttributes) => fileAttributes.WithoutFlag(FileAttributes.Hidden);

        protected static FileAttributes Hide(FileAttributes fileAttributes) => fileAttributes.WithFlag(FileAttributes.Hidden);

        protected void RemoveDirectory(DirectoryInfo directory)
        {
            var files = directory
                .GetFiles("*", SearchOption.AllDirectories)
                .ToList();
            foreach (var f in files)
            {
                _fileSystemOperator.DeleteFile(f);
                if (IsVerbose)
                {
                    Console.WriteLine($" Removed : {f.FullName}");
                }
            }

            _fileSystemOperator.DeleteDirectory(directory, true);
            Console.WriteLine($" Removed : {directory.FullName}");
        }
    }
}
