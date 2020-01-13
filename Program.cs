using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CleanupUserProfile
{
    class Program
    {
        static void Main(string[] args)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DoSomething(userProfile,
                f =>
                {
                    // ignore ntuser.* files
                    f.RemoveAll(fn => fn.Name.StartsWith("ntuser.", StringComparison.InvariantCultureIgnoreCase));

                    // yarn needs yarnrc to not be hidden to work, duh
                    CheckNotHidden(f, ".yarnrc");

                    CheckHidden(f, ".gitconfig");
                    CheckHidden(f, ".gitignore_global");
                    CheckHidden(f, ".sqltools-setup");
                    CheckHidden(f, "_lesshst");

                    Remove(f, ".bash_history");
                    Remove(f, ".csslintrc");
                    Remove(f, ".eslintrc");
                    Remove(f, ".minttyrc");
                    Remove(f, ".node_repl_history");
                    Remove(f, ".rnd");
                    Remove(f, ".viminfo");
                    Remove(f, "coffeelint.json");
                    Remove(f, "tslint.json");

                    RemovePattern(f, new Regex(@"^\.v8flags\..+\.json$", RegexOptions.IgnoreCase));
                },
                d =>
                {
                    CheckHidden(d, new Regex(@"^[_\.][\w\d\.-]+$", RegexOptions.IgnoreCase));

                    CheckHidden(d, "IntelGraphicsProfiles");
                    CheckHidden(d, "Links");
                    CheckHidden(d, "MicrosoftEdgeBackups");
                    CheckHidden(d, "OpenVPN");

                    Ignore(d, "AppData");
                    SubDirectory(d, "Documents",
                        filesActions: df => { CheckHidden(df, "Default.rdp"); }, directoriesActions: dd =>
                        {
                            Ignore(dd, "GIT");
                            Ignore(dd, "Mes sources de données");

                            Remove(dd, "Custom Office Templates");
                            Remove(dd, "Fiddler2");
                            Remove(dd, "IISExpress");
                            Remove(dd, "Mes fichiers reçus");
                            Remove(dd, "Modèles Office personnalisés");
                            Remove(dd, "My Received Files");
                            Remove(dd, "My Web Sites");
                            Remove(dd, "Outlook Files");
                            Remove(dd, "SQL Server Management Studio");
                            Remove(dd, "Visual Studio 2015");
                            Remove(dd, "Visual Studio 2017");
                            Remove(dd, "Visual Studio 2019");
                            Remove(dd, "WindowsPowerShell");
                        });
                    CheckHidden(d, "Favorites");
                    SubDirectory(d, new[] {"Google Drive", "GoogleDrive"},
                        directoriesActions: gd =>
                        {
                            Ignore(gd, "Checklist");
                            Ignore(gd, "Documents");
                            Ignore(gd, "Draft");
                            Ignore(gd, "Images");
                            Ignore(gd, "ok");
                            Ignore(gd, "Projets");
                            Ignore(gd, "TMP");

                            CheckHidden(gd, ".tmp.drivedownload");
                        });
                    Ignore(d, "OneDrive - FNAC");
                    SubDirectory(d, "Pictures", directoriesActions: pd =>
                    {
                        CheckEmptyFolderAndHide(pd, "Camera Roll");
                        CheckEmptyFolderAndHide(pd, "Saved Pictures");

                        Ignore(pd, "Screenpresso");
                    });
                    Ignore(d, "Recent");
                    CheckHidden(d, "Searches");
                    Ignore(d, "repos");
                    Ignore(d, "Wallpapers");

                    CheckEmptyFolder(d, "Desktop");
                    CheckEmptyFolder(d, "Downloads");

                    Remove(d, ".nx");
                    Remove(d, "source");

                    CheckEmptyFolderAndHide(d, "Music");
                    CheckEmptyFolderAndHide(d, "3D Objects");
                    CheckEmptyFolderAndHide(d, "Contacts");
                    CheckEmptyFolderAndHide(d, "Saved Games");

                    SubDirectory(d, "Videos", vf => { }, vd => { CheckEmptyFolderAndRemove(vd, "Captures"); },
                        videoDirectory => SetVisibility(videoDirectory, Hide));
                });
        }

        private static void RemovePattern(List<FileInfo> fileInfos, Regex pattern)
        {
            foreach (var toRemove in fileInfos.GetAndRemoveAll(pattern))
            {
                toRemove.Delete();
            }
        }

        private static void DoSomething(string folder, Action<List<FileInfo>> filesActions = null,
            Action<List<DirectoryInfo>> directoriesActions = null)
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
            if (filesActions != null)
            {
                var files = folderInfo.GetFiles().ToList();
                Ignore(files, "Desktop.ini");
                filesActions(files);
                whatToDo(files);
            }

            // dirs
            if (directoriesActions != null)
            {
                var directories = folderInfo.GetDirectories().ToList();
                directoriesActions(directories);
                whatToDo(directories);
            }
        }

        private static void Remove(List<DirectoryInfo> fileSystemInfos, string name)
        {
            if (fileSystemInfos.TryGetAndRemove(name, out var fileSystemInfo))
            {
                fileSystemInfo.GetFiles("*", SearchOption.AllDirectories).ToList()
                    .ForEach(f => f.Attributes = FileAttributes.Normal);
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

        private static DirectoryInfo CheckEmptyFolder(List<DirectoryInfo> directories, string name)
        {
            return CheckEmptyFolder(directories, name, out _);
        }

        private static DirectoryInfo CheckEmptyFolder(List<DirectoryInfo> directories, string name, out bool? empty)
        {
            if (directories.TryGetAndRemove(name, out var directory))
            {
                var files = directory
                    .GetFiles()
                    .Cast<FileSystemInfo>()
                    .Union(directory.GetDirectories())
                    .Where(fsi => !IsDesktopIni(fsi))
                    .ToList();

                empty = !files.Any();
                foreach (var file in files)
                {
                    Console.WriteLine($" Remove me : {file.FullName}");
                }

                return directory;
            }

            empty = null;
            return null;
        }

        private static void CheckEmptyFolderAndHide(List<DirectoryInfo> directories, string name)
        {
            var directory = CheckEmptyFolder(directories, name, out var empty);
            if (directory != null && empty != null)
            {
                if (empty.Value)
                {
                    SetVisibility(directory, Hide);
                }
                else
                {
                    SetVisibility(directory, Show);
                }
            }
        }

        private static void CheckEmptyFolderAndRemove(List<DirectoryInfo> directories, string name)
        {
            var directory = CheckEmptyFolder(directories, name, out var empty);
            if (directory != null && empty != null && empty.Value)
            {
                directory.Delete();
            }
        }

        private static void Ignore<T>(List<T> fileSystemInfos, string name) where T : FileSystemInfo
        {
            fileSystemInfos.TryGetAndRemove(name, out var fileToHide);
        }

        private static void SubDirectory<T>(List<T> fileSystemInfos, string name,
            Action<List<FileInfo>> filesActions = null,
            Action<List<DirectoryInfo>> directoriesActions = null,
            Action<T> subDirectoryAction = null) where T : FileSystemInfo
        {
            SubDirectory(fileSystemInfos, new[] {name}, filesActions, directoriesActions, subDirectoryAction);
        }

        private static void SubDirectory<T>(List<T> fileSystemInfos, string[] names,
            Action<List<FileInfo>> filesActions = null,
            Action<List<DirectoryInfo>> directoriesActions = null,
            Action<T> subDirectoryAction = null) where T : FileSystemInfo
        {
            foreach (var name in names)
            {
                if (fileSystemInfos.TryGetAndRemove(name, out var subDirectory))
                {
                    DoSomething(subDirectory.FullName, filesActions, directoriesActions);
                    if (subDirectoryAction != null)
                    {
                        subDirectoryAction(subDirectory);
                    }
                }
            }
        }

        private static void CheckHidden<T>(List<T> fileSystemInfos, string name) where T : FileSystemInfo
            => ModifyFileAttributes(fileSystemInfos, name, Hide);

        private static void CheckNotHidden<T>(List<T> fileSystemInfos, string name) where T : FileSystemInfo
            => ModifyFileAttributes(fileSystemInfos, name, Show);

        private static FileAttributes Show(FileAttributes fileAttributes)
        {
            return fileAttributes.WithoutFlag(FileAttributes.Hidden);
        }

        private static FileAttributes Hide(FileAttributes fileAttributes)
        {
            return fileAttributes.WithFlag(FileAttributes.Hidden);
        }

        private static void ModifyFileAttributes<T>(List<T> fileSystemInfos, string name,
            Func<FileAttributes, FileAttributes> modifyAction) where T : FileSystemInfo
        {
            if (fileSystemInfos.TryGetAndRemove(name, out var fileToModify))
            {
                SetVisibility(fileToModify, modifyAction);
            }
        }

        private static void SetVisibility<T>(T fileToModify, Func<FileAttributes, FileAttributes> modifyAction)
            where T : FileSystemInfo
        {
            var attributes = File.GetAttributes(fileToModify.FullName);
            var newAttributes = modifyAction(attributes);
            File.SetAttributes(fileToModify.FullName, newAttributes);
        }

        private static void CheckHidden<T>(List<T> fileSystemInfos, Regex namePattern) where T : FileSystemInfo
        {
            while (fileSystemInfos.TryGetAndRemove(namePattern, out var fileToModify))
            {
                var attributes = File.GetAttributes(fileToModify.FullName);
                var newAttributes = attributes.WithFlag(FileAttributes.Hidden);
                File.SetAttributes(fileToModify.FullName, newAttributes);
            }
        }
    }
}