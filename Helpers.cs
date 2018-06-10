using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
    }
}