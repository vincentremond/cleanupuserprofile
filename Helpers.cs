using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CleanupUserProfile
{
    public static class Helpers
    {
        public static bool TryGetAndRemove<T>(this List<T> fi, string name, out T result) where T : FileSystemInfo
        {
            result = fi.SingleOrDefault(f => string.Equals(f.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                fi.Remove(result);
            }

            return result != null;
        }

        public static bool TryGetAndRemove<T>(this List<T> fi, Regex namePattern, out T result) where T : FileSystemInfo
        {
            result = fi.FirstOrDefault(f => namePattern.IsMatch(f.Name));
            if (result != null)
            {
                fi.Remove(result);
            }

            return result != null;
        }

        public static IEnumerable<T> GetAndRemoveAll<T>(this List<T> fi, Regex namePattern) where T : FileSystemInfo
        {
            var toRemove = new List<T>();
            foreach (var f in fi)
            {
                if (namePattern.IsMatch(f.Name))
                {
                    toRemove.Add(f);
                    yield return f;
                }
            }

            foreach (var t in toRemove)
            {
                fi.Remove(t);
            }
        }

        public static T WithFlag<T>(this T value, T add)
        {
            return (T) (object) ((int) (object) value | (int) (object) add);
        }

        public static T WithoutFlag<T>(this T value, T remove)
        {
            return (T) (object) ((int) (object) value & ~(int) (object) remove);
        }
    }
}