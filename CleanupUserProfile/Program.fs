﻿open System
open System.IO
open System.Text.RegularExpressions
open Pinicola.FSharp.SpectreConsole

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
    | DeleteRecursive
    | ContainsNoFiles

and Condition =
    | Any
    | Name of StringRule
    | Extension of StringRule
    | IsSymLink
    | IsHidden
    | IsReadOnly
    | IsSystem
    | LastWriteTime of DateTimeCondition
    | And of Condition list
    | Or of Condition list
    | Not of Condition

and StringRule =
    | StartsWith of string
    | Match of string
    | Eq of string

and DateTimeCondition =
    | OlderThan of TimeSpan
    | NewerThan of TimeSpan
    | Before of DateTime
    | After of DateTime

let (<&&>) a b =
    And [
        a
        b
    ]

let (<||>) a b =
    Or [
        a
        b
    ]

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

let testDateTimeCondition lastWriteTime dateTimeCondition =
    match dateTimeCondition with
    | Before dateTime -> lastWriteTime < dateTime
    | After dateTime -> lastWriteTime > dateTime
    | NewerThan duration -> (DateTime.Now - lastWriteTime) < duration
    | OlderThan duration -> (DateTime.Now - lastWriteTime) > duration

let rec testCondition rule (item: FileSystemInfo) =
    match rule with
    | Any -> true
    | Name stringRule -> testStringRule item.Name stringRule
    | IsSymLink -> item.Attributes.HasFlag(FileAttributes.ReparsePoint)
    | IsHidden -> item.Attributes.HasFlag(FileAttributes.Hidden)
    | IsReadOnly -> item.Attributes.HasFlag(FileAttributes.ReadOnly)
    | IsSystem -> item.Attributes.HasFlag(FileAttributes.System)
    | Extension stringRule -> testStringRule item.Extension stringRule
    | LastWriteTime dateTimeCondition -> testDateTimeCondition item.LastWriteTime dateTimeCondition
    | And rules -> rules |> List.forall (fun r -> testCondition r item)
    | Or rules -> rules |> List.exists (fun r -> testCondition r item)
    | Not rule -> not <| testCondition rule item

let applyAction (action: Action) (item: FileSystemInfo) =
    match action with
    | Hide ->
        if not <| item.Attributes.HasFlag(FileAttributes.Hidden) then
            AnsiConsole.markupLineInterpolated $"[blue]Hiding[/] \"[bold white]{item.FullName}[/]\""
            item.Attributes <- item.Attributes ||| FileAttributes.Hidden
    | Noop -> ()
    | Unlink ->
        AnsiConsole.markupLineInterpolated $"[blue]Unlinking[/] \"[bold white]{item.FullName}[/]\""
        item.Delete()
    | DeleteRecursive ->
        match item with
        | :? DirectoryInfo as dir ->
            AnsiConsole.markupLineInterpolated $"[blue]Deleting dir recursively[/] \"[bold white]{item.FullName}[/]\""
            dir.Delete(true)
        | _ -> failwith "Cannot delete recursively something that is not a DirectoryInfo"
    | ContainsNoFiles ->
        match item with
        | :? DirectoryInfo as dir ->
            if Array.isEmpty (dir.GetFiles("*", SearchOption.AllDirectories)) then
                AnsiConsole.markupLineInterpolated $"[blue]Deleting dir[/] \"[bold white]{item.FullName}[/]\""
                dir.Delete(true)
            else
                failwith $"Cannot delete dir \"{item.FullName}\" because it contains files"
        | _ -> failwith "Cannot delete something that is not a DirectoryInfo"
    | Delete ->
        match item with
        | :? DirectoryInfo as dir ->
            AnsiConsole.markupLineInterpolated $"[blue]Deleting dir[/] \"[bold white]{item.FullName}[/]\""
            dir.Delete(false)
        | :? FileInfo as file ->
            AnsiConsole.markupLineInterpolated $"[blue]Deleting file[/] \"[bold white]{item.FullName}[/]\""
            file.Delete()
        | _ -> failwith "Cannot delete something that is not a DirectoryInfo or a FileInfo"

let rec run (folder: DirectoryInfo) (foldersRules: FolderRule list) (filesRules: FileRule list) =
    (runFolders folder foldersRules) @ (runFiles folder filesRules)

and runFolders folder foldersRules : FileSystemInfo list =
    folder.GetDirectories()
    |> Seq.collect (fun dirInfo ->
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

            let childResults =
                match folderRule.FolderChildRules with
                | FolderChildRules.Process(foldersRules, filesRules) -> run dirInfo foldersRules filesRules
                | FolderChildRules.Noop -> []

            // If no issue with the child, apply the action on the folder
            if List.isEmpty childResults then
                applyAction folderRule.SelfAction dirInfo

            childResults

        | None -> [ (dirInfo :> FileSystemInfo) ]
    )
    |> Seq.toList

and runFiles folder filesRules : FileSystemInfo list =
    folder.GetFiles()
    |> Seq.choose (fun fileInfo ->

        if testCondition (Name(Eq "desktop.ini")) fileInfo then
            None
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
            | Some fileRule ->
                applyAction fileRule.Action fileInfo
                None
            | None -> Some(fileInfo :> FileSystemInfo)
    )
    |> Seq.toList

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

let notProcessedItems =
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
                Name(Eq "OneDrive")
                Name(StartsWith "OneDrive -")
            ])
            Noop
        dir (Name(Eq "TMP")) Noop [
            dir (Name(Match "^\d{4}$")) Noop [] []
            dir (Name(Match "^\d{4}-\d{2}$")) Noop [ dir' (Name(Match "^\d{4}-\d{2}-\d{2}-")) Noop ] []
        ] []
        dir (Name(Eq "Downloads")) Noop [] []
        dir'
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
            Unlink
        dir' (Name(Eq "dotTraceSnapshots")) DeleteRecursive
        dir' (Name(Eq "RiderSnapshots")) DeleteRecursive
        dir (Name(Eq "Contacts")) Hide [] []
        dir (Name(Eq "Links")) Hide [] [ file (Extension(Eq ".lnk")) Noop ]
        dir (Name(Eq "Pictures")) Hide [
            dir' (Name(Eq "Screenpresso")) Noop
            dir (Name(Eq "Wallpapers")) Noop [] []
            dir (Name(Eq "Camera Roll")) Noop [] []
            dir (Name(Eq "Saved Pictures")) Noop [] []
            dir (Name(Eq "Feedback")) Delete [
                dir (Name(Match @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$")) Delete [] [
                    file (Name(Match @"\.png$")) Delete
                ]
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
            file (Extension(Eq ".appref-ms")) Delete
            file (Extension(Eq ".url")) Delete
        ]
        dir' (Name(Eq "Favorites")) Noop
        dir' (Name(Eq "IntelGraphicsProfiles")) Hide
        dir' (Name(Eq "Perso")) Noop
        dir (Name(Eq "Postman")) Delete [ dir (Name(Eq "files")) Delete [] [] ] []
        dir (Name(Eq "nuget")) Noop [] [ file (Extension(Eq ".nupkg")) Noop ]
        dir (Name(Eq "source")) Delete [ dir (Name(Eq "repos")) Delete [] [] ] []
        dir (Name(Eq "Documents")) Noop [
            dir (Name(Eq "Power BI Desktop")) Delete [ dir (Name(Eq "Custom Connectors")) Delete [] [] ] []
            dir (Name(Eq "FeedbackHub")) Delete [] []
            dir (Name(Eq "Custom Office Templates")) Delete [] []
            dir' (Name(Eq "KerialisLogs")) Noop
            dir' (Name(Eq "PICRIS")) Noop
            dir' (Name(Eq "Fiddler2")) Noop
            dir' (Name(Eq "Dell")) Noop
            dir (Name(Eq "PowerToys")) Delete [ dir (Name(Eq "Backup")) Delete [] [] ] []
            dir'
                (And [
                    IsSymLink
                    Or [
                        Name(Eq "Ma musique")
                        Name(Eq "Mes images")
                        Name(Eq "Mes vidéos")
                        Name(Eq "My Music")
                        Name(Eq "My Pictures")
                        Name(Eq "My Videos")
                    ]
                ])
                Unlink
            dir'
                (Or [
                    Name(Eq "IISExpress")
                    Name(Eq "PowerShell")
                    Name(Eq "WindowsPowerShell")
                ])
                Noop
            dir'
                (Or [
                    Name(Eq "My Web Sites")
                    Name(Eq "Visual Studio 2017")
                    Name(Eq "Visual Studio 2022")
                    Name(Eq "SQL Server Management Studio")
                    Name(Eq "Modèles Office personnalisés")
                ])
                ContainsNoFiles
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

        file (Name(Eq "java_error_in_rider64.hprof")) Delete
        file (Extension(Eq ".mdf")) Delete
        file (Extension(Eq ".ldf")) Delete
        file (Name(StartsWith @"NTUSER.")) Noop
        file
            (Name(Match @"(_log)?.(ldf|mdf)$")
             <&&> LastWriteTime(OlderThan(TimeSpan.FromDays(1.))))
            Hide
    ]

let exitCode =
    match notProcessedItems with
    | [] ->
        AnsiConsole.markupLine "[green]No manual action required[/]"
        0
    | notProcessedItems ->
        for item in notProcessedItems do
            AnsiConsole.markupLineInterpolated $"[red]What to do with[/] \"[bold white]{item.FullName}[/]\""

        1

Environment.Exit(exitCode)
