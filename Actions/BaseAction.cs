using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CleanupUserProfile.Actions
{
    internal abstract class BaseAction : IAction
    {
        private readonly Regex _pattern;

        protected BaseAction(
            string pattern)
        {
            if (pattern != null) _pattern = ToRegex(pattern);
        }

        public bool IsMatch(
            FileSystemInfo fileInfo)
        {
            return _pattern.IsMatch(fileInfo.Name);
        }

        public abstract void Execute(
            FileSystemInfo file);

        private static Regex ToRegex(
            string rule)
        {
            const string regexPrefix = "REGEX:";
            var pattern = rule.StartsWith(regexPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? rule.Substring(regexPrefix.Length)
                : $"^{Regex.Escape(rule)}$";

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected static bool IsDesktopIni(
            FileSystemInfo file)
        {
            return file is FileInfo x && x.Name.Equals("Desktop.ini", StringComparison.InvariantCultureIgnoreCase);
        }

        protected static void SetVisibility<T>(
            T fileToModify,
            Func<FileAttributes, FileAttributes> modifyAction)
            where T : FileSystemInfo
        {
            var attributes = File.GetAttributes(fileToModify.FullName);
            var newAttributes = modifyAction(attributes);
            File.SetAttributes(fileToModify.FullName, newAttributes);
        }

        protected static FileAttributes Show(
            FileAttributes fileAttributes)
        {
            return fileAttributes.WithoutFlag(FileAttributes.Hidden);
        }

        protected static FileAttributes Hide(
            FileAttributes fileAttributes)
        {
            return fileAttributes.WithFlag(FileAttributes.Hidden);
        }
    }
}