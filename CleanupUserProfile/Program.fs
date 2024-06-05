open System
open System.IO
open System.Text.RegularExpressions

type FileRule = {
    Condition: Condition
    Action: Action
}

and FolderRule = {
    Condition: Condition
    SelfAction: Action
    FolderChildRules: FolderChildRules
}

and FolderChildRules =
    | Noop
    | Process of FolderRule list * FileRule list

and Action =
    | Hide
    | Noop
    | Unlink
    | Delete

and Condition =
    | Any
    | Name of StringRule
    | Extension of StringRule
    | IsSymLink
    | IsHidden
    | IsReadOnly
    | IsSystem
    | And of Condition list
    | Or of Condition list
    | Not of Condition

and StringRule =
    | StartsWith of string
    | Match of string
    | Eq of string

let (</>) a b = Path.Combine(a, b)

let specialFolders = {|
    UserProfile =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        |> DirectoryInfo
    Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) |> DirectoryInfo
|}

let userProfile =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    |> DirectoryInfo

[<RequireQualifiedAccess>]
module Regex =
    let isMatch (i: string) (r: Regex) = r.IsMatch(i)

[<RequireQualifiedAccess>]
module String =
    let startsWith (str: string) (value: string) =
        str.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)

    let startsWithCaseSensitive (str: string) (value: string) =
        str.StartsWith(value, StringComparison.CurrentCulture)

    let equals (str: string) (value: string) =
        str.Equals(value, StringComparison.CurrentCultureIgnoreCase)

    let equalsCaseSensitive (str: string) (value: string) =
        str.Equals(value, StringComparison.CurrentCulture)

let testStringRule str stringRule =
    match stringRule with
    | StartsWith prefix -> String.startsWith str prefix
    | Match pattern -> pattern |> Regex |> Regex.isMatch str
    | Eq value -> String.equals str value

let rec testCondition rule (item: FileSystemInfo) =
    match rule with
    | Any -> true
    | Name stringRule -> testStringRule item.Name stringRule
    | IsSymLink -> item.Attributes.HasFlag(FileAttributes.ReparsePoint)
    | IsHidden -> item.Attributes.HasFlag(FileAttributes.Hidden)
    | IsReadOnly -> item.Attributes.HasFlag(FileAttributes.ReadOnly)
    | IsSystem -> item.Attributes.HasFlag(FileAttributes.System)
    | Extension stringRule -> testStringRule item.Extension stringRule
    | And rules -> rules |> List.forall (fun r -> testCondition r item)
    | Or rules -> rules |> List.exists (fun r -> testCondition r item)
    | Not rule -> not <| testCondition rule item

let applyAction (action: Action) (item: FileSystemInfo) =
    match action with
    | Hide ->
        if not <| item.Attributes.HasFlag(FileAttributes.Hidden) then
            printfn $"Hiding \"{item.FullName}\""
            item.Attributes <- item.Attributes ||| FileAttributes.Hidden
    | Noop -> ()
    | Unlink ->
        printfn $"Unlinking \"{item.FullName}\""
        item.Delete()
    | Delete ->
        match item with
        | :? DirectoryInfo as dir ->
            printfn $"Deleting dir \"{item.FullName}\""
            dir.Delete(false)
        | :? FileInfo as file ->
            printfn $"Deleting file \"{item.FullName}\""
            file.Delete()
        | _ -> failwith "Cannot delete something that is not a DirectoryInfo or a FileInfo"

let rec run (folder: DirectoryInfo) (foldersRules: FolderRule list) (filesRules: FileRule list) =

    folder.GetDirectories()
    |> Seq.iter (fun dirInfo ->
        let rule =
            foldersRules
            |> List.tryPick (fun folderRule ->
                if testCondition folderRule.Condition dirInfo then
                    Some folderRule
                else
                    None
            )

        match rule with
        | Some folderRule ->

            match folderRule.FolderChildRules with
            | FolderChildRules.Process(foldersRules, filesRules) -> run dirInfo foldersRules filesRules
            | FolderChildRules.Noop -> ()

            applyAction folderRule.SelfAction dirInfo

        | None -> printfn $"What to do with DIR  {dirInfo}"
    )

    folder.GetFiles()
    |> Seq.iter (fun fileInfo ->

        if testCondition (Name(Eq "desktop.ini")) fileInfo then
            ()
        else

            let rule =
                filesRules
                |> List.tryPick (fun fileRule ->
                    if testCondition fileRule.Condition fileInfo then
                        Some fileRule
                    else
                        None
                )

            match rule with
            | Some fileRule -> applyAction fileRule.Action fileInfo
            | None -> printfn $"What to do with FILE {fileInfo}"
    )

let file condition action = {
    Condition = condition
    Action = action
}

let dir condition action childFoldersRules childFilesRules = {
    Condition = condition
    SelfAction = action
    FolderChildRules = FolderChildRules.Process(childFoldersRules, childFilesRules)
}

let dir' condition action = {
    Condition = condition
    SelfAction = action
    FolderChildRules = FolderChildRules.Noop
}

let ignoreFolders = [ dir Any ]

run userProfile [
    dir'
        (Or [
            Name(StartsWith ".")
            Name(StartsWith "_")
        ])
        Hide
    dir'
        (Or [
            Name(Eq "AppData")
            Name(Eq "Apps")
            Name(Eq "Data")
            Name(Eq "repos")
            Name(Eq "tmp")
            Name(Eq "OneDrive")
            Name(StartsWith "OneDrive -")
        ])
        Noop
    dir (Name(Eq "Downloads")) Noop [] []
    dir
        (And [
            IsSymLink
            Or [
                Name(Eq @"Application Data")
                Name(Eq @"Cookies")
                Name(Eq @"Local Settings")
                Name(Eq @"Menu Démarrer")
                Name(Eq @"Mes documents")
                Name(Eq @"Modèles")
                Name(Eq @"My Documents")
                Name(Eq @"NetHood")
                Name(Eq @"PrintHood")
                Name(Eq @"Recent")
                Name(Eq @"SendTo")
                Name(Eq @"Start Menu")
                Name(Eq @"Templates")
                Name(Eq @"Voisinage d'impression")
                Name(Eq @"Voisinage réseau")
            ]
        ])
        Unlink [] []
    dir (Name(Eq "dotTraceSnapshots")) Delete [
        dir (Name(Eq "Temp")) Delete [] []
        dir (Name(StartsWith "Unit Tests ")) Delete [] [ file (Name(Match @"\.tmp(\.\d+)?$")) Delete ]
    ] []
    dir (Name(Eq "Contacts")) Hide [] []
    dir (Name(Eq "Links")) Hide [] [ file (Extension(Eq ".lnk")) Noop ]
    dir (Name(Eq "Pictures")) Hide [
        dir' (Name(Eq "Screenpresso")) Noop
        dir (Name(Eq "Wallpapers")) Noop [] []
        dir (Name(Eq "Camera Roll")) Noop [] []
        dir (Name(Eq "Saved Pictures")) Noop [] []
        dir (Name(Eq "Feedback")) Delete [
            dir (Name(Match @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$")) Delete [] [ file (Name(Match @"\.png$")) Delete ]
        ] []
    ] []
    dir (Name(Eq "Music")) Hide [] []
    dir (Name(Eq "Videos")) Hide [
        dir (Name(Eq "Captures")) Noop [] [ file (Extension(Eq ".mp4")) Delete ]
        dir (Name(Eq "AnyDesk")) Delete [] []
    ] []
    dir (Name(Eq "Searches")) Hide [] [
        file (Extension(Eq ".search-ms")) Noop
        file (Extension(Eq ".searchconnector-ms")) Noop
    ]
    dir (Name(Eq "Saved Games")) Hide [] []
    dir (Name(Eq "ai_overlay_tmp")) Hide [] []
    dir (Name(Eq "Desktop")) Noop [] [
        file (Extension(Eq ".lnk")) Delete
        file (Extension(Eq ".url")) Delete
    ]
    dir' (Name(Eq "Favorites")) Noop
    dir' (Name(Eq "IntelGraphicsProfiles")) Hide
    dir' (Name(Eq "Perso")) Noop
    dir (Name(Eq "Postman")) Delete [ dir (Name(Eq "files")) Delete [] [] ] []
    dir (Name(Eq "nuget")) Noop [] [ file (Extension(Eq ".nupkg")) Noop ]
    dir (Name(Eq "source")) Delete [ dir (Name(Eq "repos")) Delete [] [] ] []
    dir (Name(Eq "Documents")) Noop [
        dir (Name(Eq "Custom Office Templates")) Delete [] []
        dir' (Name(Eq "Fiddler2")) Noop
        dir' (Name(Eq "Dell")) Noop
        dir (Name(Eq "PowerToys")) Delete [ dir (Name(Eq "Backup")) Delete [] [] ] []
        dir
            (Or [
                Name(Eq "Ma musique")
                Name(Eq "Mes images")
                Name(Eq "Mes vidéos")
                Name(Eq "My Music")
                Name(Eq "My Pictures")
                Name(Eq "My Videos")
            ])
            Unlink [] []
        dir'
            (Or [
                Name(Eq "IISExpress")
                Name(Eq "PowerShell")
                Name(Eq "WindowsPowerShell")
                Name(Eq "My Web Sites")
                Name(Eq "Visual Studio 2017")
                Name(Eq "Visual Studio 2022")
            ])
            Noop
        dir (Name(Eq "Fichiers Outlook")) Noop [] []
        dir (Name(Eq "Zoom")) Noop [] []
    ] [ file (Name(Eq "Default.rdp")) Hide ]
] [
    file (Name(Eq ".editorconfig")) Noop
    file
        (Or [
            Name(StartsWith ".")
            Name(StartsWith "_")
        ])
        Hide

    file (Name(StartsWith @"NTUSER.")) Noop
    file (Name(Match @"^AzureStorageEmulatorDb\d+(_log)?.(ldf|mdf)$")) Hide
]
