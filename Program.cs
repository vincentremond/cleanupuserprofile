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
            DoSomething(userProfile,
            f =>
            {
                f.RemoveAll(fn => fn.Name.StartsWith("ntuser.", StringComparison.InvariantCultureIgnoreCase));
                CheckHidden(f, ".gitconfig");
            },
            d =>
            {
                CheckHidden(d, ".dotnet");
                CheckHidden(d, ".omnisharp");
                CheckHidden(d, ".nuget");
                CheckHidden(d, ".templateengine");
                CheckHidden(d, ".vscode");
                CheckHidden(d, "IntelGraphicsProfiles");
                CheckHidden(d, "MicrosoftEdgeBackups");
                CheckHidden(d, "OpenVPN");

                Ignore(d, "AppData");
                Ignore(d, "Google Drive");
                Ignore(d, "Documents");
                Ignore(d, "Pictures");
                Ignore(d, "Recent");
                Ignore(d, "Searches");
                Ignore(d, "Wallpapers");

                CheckEmptyFolder(d, "Desktop");
                CheckEmptyFolder(d, "Downloads");
                CheckEmptyFolder(d, "Favorites");
                CheckEmptyFolder(d, "Links");

                Remove(d, "source");
            });

            DoSomething(Path.Combine(userProfile, "Documents"),
            f =>
            {
                CheckHidden(f, "Default.rdp");
            }, d =>
            {
                Remove(d, "Visual Studio 2017");
                Ignore(d, "GIT");
            });

            DoSomething(Path.Combine(userProfile, "Pictures"),
            f =>
            {
            }, d =>
            {
                CheckEmptyFolder(d, "Camera Roll");
                CheckEmptyFolder(d, "Saved Pictures");
            });

            DoSomething(Path.Combine(userProfile, "Google Drive"), f =>
            {

            }, d =>
            {
                Ignore(d, "Checklist");
                Ignore(d, "Documents");
                Ignore(d, "Draft");
                Ignore(d, "Images");
                Ignore(d, "ok");
                Ignore(d, "Projets");
                Ignore(d, "TMP");

                CheckHidden(d, ".tmp.drivedownload");
            });
        }

        private static void DoSomething(string folder, Action<List<FileInfo>> filesActions, Action<List<DirectoryInfo>> directoriesActions)
        {
            void whatToDo(IEnumerable<FileSystemInfo> fileSystemInfos)
            {
                foreach (var fileSystemInfo in fileSystemInfos)
                {
                    Console.WriteLine($" What to do ? {fileSystemInfo.GetType().Name} > {fileSystemInfo.FullName}");
                }
            }

            var folderInfo = new DirectoryInfo(folder);
            if(!folderInfo.Exists)
            {
                return;
            }
            // files
            var files = folderInfo.GetFiles().ToList();
            Ignore(files, "Desktop.ini");
            filesActions(files);
            whatToDo(files);
            // dirs
            var directories = folderInfo.GetDirectories().ToList();
            directoriesActions(directories);
            whatToDo(directories);
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

        private static void Ignore<T>(List<T> fileSystemInfos, string name) where T : FileSystemInfo
        {
            fileSystemInfos.TryGetAndRemove(name, out var fileToHide);
        }

        private static void CheckHidden<T>(List<T> fileSystemInfos, string name) where T : FileSystemInfo
        {
            if (fileSystemInfos.TryGetAndRemove(name, out var fileToHide))
            {
                File.SetAttributes(fileToHide.FullName, FileAttributes.Hidden);
            }
        }
    }
}
