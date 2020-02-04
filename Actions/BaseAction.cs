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
            if (pattern != null)
            {
                _pattern = ToRegex(pattern);
            }
        }

        private Regex ToRegex(
            string rule)
        {
            const string regexPrefix = "REGEX:";
            var pattern = rule.StartsWith(regexPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? rule.Substring(regexPrefix.Length)
                : $"^{Regex.Escape(rule)}$";

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public bool IsMatch(
            FileSystemInfo fileInfo)
        {
            return _pattern.IsMatch(fileInfo.Name);
        }

        public abstract void Execute(
            FileSystemInfo file);
    }
}