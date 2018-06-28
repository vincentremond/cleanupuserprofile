﻿using System;
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
                // ignore ntuser.* files
                f.RemoveAll(fn => fn.Name.StartsWith("ntuser.", StringComparison.InvariantCultureIgnoreCase));

                CheckHidden(f, ".gitconfig");
                CheckHidden(f, ".yarnrc");

                Remove(f, ".bash_history");
                Remove(f, ".csslintrc");
                Remove(f, ".eslintrc");
                Remove(f, ".node_repl_history");
                Remove(f, ".rnd");
                Remove(f, ".viminfo");
                Remove(f, "coffeelint.json");
                Remove(f, "tslint.json");

            },
            d =>
            {
                CheckHidden(d, ".dotnet");
                CheckHidden(d, ".config");
                CheckHidden(d, ".omnisharp");
                CheckHidden(d, ".nuget");
                CheckHidden(d, ".templateengine");
                CheckHidden(d, ".vscode");
                CheckHidden(d, "IntelGraphicsProfiles");
                CheckHidden(d, "MicrosoftEdgeBackups");
                CheckHidden(d, "OpenVPN");

                Ignore(d, "AppData");
                Ignore(d, "GoogleDrive");
                Ignore(d, "Google Drive");
                Ignore(d, "Documents");
                Ignore(d, "Pictures");
                Ignore(d, "Recent");
                Ignore(d, "Searches");
                Ignore(d, "Wallpapers");
                Ignore(d, "Favorites");
                Ignore(d, "Links");
                Ignore(d, "OneDrive - FNAC");

                CheckEmptyFolder(d, "Desktop");
                CheckEmptyFolder(d, "Downloads");
                CheckEmptyFolder(d, "Music");

                Remove(d, ".nx");
                Remove(d, "source");
            });

            DoSomething(Path.Combine(userProfile, "Documents"),
            f =>
            {
                CheckHidden(f, "Default.rdp");
            }, d =>
            {
                Ignore(d, "GIT");
                
                Remove(d, "Custom Office Templates");
                Remove(d, "Fiddler2");
                Remove(d, "My Received Files");
                Remove(d, "My Web Sites");
                Remove(d, "Outlook Files");
                Remove(d, "SQL Server Management Studio");
                Remove(d, "Visual Studio 2015");
                Remove(d, "Visual Studio 2017");
            });

            DoSomething(Path.Combine(userProfile, "Pictures"),
            f =>
            {
            }, d =>
            {
                CheckEmptyFolder(d, "Camera Roll");
                CheckEmptyFolder(d, "Saved Pictures");

                Ignore(d, "Screenpresso");
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
            if (!folderInfo.Exists)
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

        private static void Remove(List<DirectoryInfo> fileSystemInfos, string name)
        {
            if (fileSystemInfos.TryGetAndRemove(name, out var fileSystemInfo))
            {
                fileSystemInfo.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(f => f.Attributes = FileAttributes.Normal);
                fileSystemInfo.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(f => f.Delete());
                fileSystemInfo.Delete(true);
            }
        }

        private static void Remove(List<FileInfo> fileSystemInfos, string name)
        {
            if (fileSystemInfos.TryGetAndRemove(name, out var fileSystemInfo))
            {
                fileSystemInfo.Delete();
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
