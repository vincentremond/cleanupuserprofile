using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Environment;

namespace CleanupUserProfile
{
    class Program
    {
        static void Main(string[] args)
        {
            var userProfile = Environment.GetFolderPath(SpecialFolder.UserProfile);
            {
                var files = Directory
                    .GetFiles(userProfile, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => new FileInfo(f))
                    .ToList()
                    ;

                files.RemoveAll(f => f.Name.StartsWith("ntuser.", StringComparison.InvariantCultureIgnoreCase));
                CheckHidden(files, ".gitconfig");

                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }

                var directories = Directory
                    .GetDirectories(userProfile, "*", SearchOption.TopDirectoryOnly)
                    .Select(d => new DirectoryInfo(d))
                    .ToList();

                CheckHidden(directories, ".dotnet");
                CheckHidden(directories, ".omnisharp");
                CheckHidden(directories, ".templateengine");
                CheckHidden(directories, ".vscode");
                CheckHidden(directories, "IntelGraphicsProfiles");
                CheckHidden(directories, "MicrosoftEdgeBackups");
                CheckHidden(directories, "OpenVPN");

                Ignore(directories, "AppData");
                Ignore(directories, "Google Drive");
                Ignore(directories, "Documents");
                Ignore(directories, "Recent");
                Ignore(directories, "Searches");
                Ignore(directories, "Wallpapers");

                CheckEmptyFolder(directories, "Desktop");
                CheckEmptyFolder(directories, "Downloads");
                CheckEmptyFolder(directories, "Favorites");
                CheckEmptyFolder(directories, "Links");
                CheckEmptyFolder(directories, "Pictures");

                foreach (var directory in directories)
                {
                    Console.WriteLine($" What to do ? > {directory.FullName}");
                }
            }
            {
                var documentsFolder = Path.Combine(userProfile, "Documents");
                var documentsFolderInfo = new DirectoryInfo(documentsFolder);

                var files = documentsFolderInfo.GetFiles().ToList();
                foreach (var file in files)
                {
                    if (IsDesktopIni(file))
                    {
                        continue;
                    }
                    Console.WriteLine($" What to do ? > {file.FullName}");
                }

                var directories = documentsFolderInfo.GetDirectories().ToList();

                Remove(directories, "Visual Studio 2017");
                Ignore(directories, "GIT");

                foreach (var directory in directories)
                {
                    Console.WriteLine($" What to do ? > {directory.FullName}");
                }
            }
        }

        private static void Remove(List<DirectoryInfo> directories, string name)
        {
            if (directories.TryGetAndRemove(name, out var directory))
            {
                directory.Delete(true);
            }
        }

        private static bool IsDesktopIni(FileSystemInfo file)
        {
            return (file is FileInfo x && x.Name.Equals("Desktop.ini", StringComparison.InvariantCultureIgnoreCase));
        }

        private static void CheckEmptyFolder(List<DirectoryInfo> directories, string name)
        {
            if (directories.TryGetAndRemove(name, out var directory))
            {
                var files = directory
                    .GetFiles()
                    .Cast<FileSystemInfo>()
                    .Union(directory.GetDirectories());
                foreach (var file in files)
                {
                    if (IsDesktopIni(file))
                    {
                        continue;
                    }
                    Console.WriteLine($" Remove me : {file.FullName}");
                }
            }
        }

        private static void Ignore<T>(List<T> directories, string name) where T : FileSystemInfo
        {
            directories.TryGetAndRemove(name, out var fileToHide);
        }

        private static void CheckHidden<T>(List<T> files, string name) where T : FileSystemInfo
        {
            if (files.TryGetAndRemove(name, out var fileToHide))
            {
                File.SetAttributes(fileToHide.FullName, FileAttributes.Hidden);
            }
        }
    }
}
