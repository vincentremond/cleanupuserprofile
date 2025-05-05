open System
open System.IO
open System.Text.RegularExpressions
open Pinicola.FSharp.IO
open Pinicola.FSharp.SpectreConsole

type FileRule = {
    Condition: Condition
    Action: FileAction
}

and DirectoryRule = {
    Condition: Condition
    SelfAction: DirectoryAction
    DirectoryChildRules: DirectoryChildRules
}

and DirectoryChildRules =
    | Noop
    | Process of DirectoryRule list * FileRule list

and FileAction =
    | Hide
    | Noop
    | Unlink
    | Delete
    | TryDelete
    | Move of MoveDestination

and DirectoryAction =
    | Hide
    | Noop
    | Unlink
    | Delete
    | DeleteRecursive
    | ContainsNoFiles

and MoveDestination = | SubDirectory of string

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
    | RegexMatch of string
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

type D = DirectoryAction
type F = FileAction

let specialDirectories = {|
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
    | RegexMatch pattern -> pattern |> Regex |> Regex.isMatch str
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

let applyDirectoryAction (action: DirectoryAction) (directoryInfo: DirectoryInfo) =
    match action with
    | D.Hide ->
        if not <| directoryInfo.Attributes.HasFlag(FileAttributes.Hidden) then
            AnsiConsole.markupLineInterpolated $"[blue]Hiding[/] \"[bold white]{directoryInfo.FullName}[/]\""
            directoryInfo.Attributes <- directoryInfo.Attributes ||| FileAttributes.Hidden
    | D.Noop -> ()
    | D.Unlink ->
        AnsiConsole.markupLineInterpolated $"[blue]Unlinking[/] \"[bold white]{directoryInfo.FullName}[/]\""
        directoryInfo.Delete()
    | D.DeleteRecursive ->
        AnsiConsole.markupLineInterpolated $"[blue]Deleting dir recursively[/] \"[bold white]{directoryInfo.FullName}[/]\""
        directoryInfo.Delete(true)
    | D.ContainsNoFiles ->
        if Array.isEmpty (directoryInfo.GetFiles("*", SearchOption.AllDirectories)) then
            AnsiConsole.markupLineInterpolated $"[blue]Deleting dir[/] \"[bold white]{directoryInfo.FullName}[/]\""
            directoryInfo.Delete(true)
        else
            failwith $"Cannot delete dir \"{directoryInfo.FullName}\" because it contains files"
    | D.Delete ->
        AnsiConsole.markupLineInterpolated $"[blue]Deleting dir[/] \"[bold white]{directoryInfo.FullName}[/]\""
        directoryInfo.Delete(false)

let applyFileAction (action: FileAction) (fileInfo: FileInfo) =
    match action with
    | F.Hide ->
        if not <| fileInfo.Attributes.HasFlag(FileAttributes.Hidden) then
            AnsiConsole.markupLineInterpolated $"[blue]Hiding[/] \"[bold white]{fileInfo.FullName}[/]\""
            fileInfo.Attributes <- fileInfo.Attributes ||| FileAttributes.Hidden
    | F.Noop -> ()
    | F.Unlink ->
        AnsiConsole.markupLineInterpolated $"[blue]Unlinking[/] \"[bold white]{fileInfo.FullName}[/]\""
        fileInfo.Delete()
    | F.Delete ->
        AnsiConsole.markupLineInterpolated $"[blue]Deleting file[/] \"[bold white]{fileInfo.FullName}[/]\""
        fileInfo.Delete()
    | F.TryDelete ->
        AnsiConsole.markupLineInterpolated $"[blue]Trying to delete file[/] \"[bold white]{fileInfo.FullName}[/]\""

        try
            fileInfo.Delete()
        with
        | :? IOException as ex -> AnsiConsole.markupLineInterpolated $"[yellow]Cannot delete file[/] \"[bold white]{fileInfo.FullName}[/]\" because {ex.Message}"
        | ex -> raise ex
    | F.Move target ->
        let computedTargetDirectory =
            match target with
            | SubDirectory subDir -> fileInfo.Directory.FullName </> subDir

        Directory.ensureExists computedTargetDirectory

        let targetFileFullPath = computedTargetDirectory </> fileInfo.Name

        AnsiConsole.markupLineInterpolated $"[blue]Moving file[/] \"[bold white]{fileInfo.FullName}[/]\" to \"[bold white]{target}[/]\""
        fileInfo.MoveTo(targetFileFullPath, overwrite = false)

let rec run (directory: DirectoryInfo) (directorysRules: DirectoryRule list) (filesRules: FileRule list) =
    (runDirectorys directory directorysRules) @ (runFiles directory filesRules)

and runDirectorys directory directorysRules : FileSystemInfo list =
    directory.GetDirectories()
    |> Seq.collect (fun dirInfo ->
        let rule =
            directorysRules
            |> List.tryPick (fun directoryRule ->
                if testCondition directoryRule.Condition dirInfo then
                    Some directoryRule
                else
                    None
            )

        match rule with
        | Some directoryRule ->

            let childResults =
                match directoryRule.DirectoryChildRules with
                | DirectoryChildRules.Process(directorysRules, filesRules) -> run dirInfo directorysRules filesRules
                | DirectoryChildRules.Noop -> []

            // If no issue with the child, apply the action on the directory
            if List.isEmpty childResults then
                applyDirectoryAction directoryRule.SelfAction dirInfo

            childResults

        | None -> [ (dirInfo :> FileSystemInfo) ]
    )
    |> Seq.toList

and runFiles directory filesRules : FileSystemInfo list =
    directory.GetFiles()
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
                applyFileAction fileRule.Action fileInfo
                None
            | None -> Some(fileInfo :> FileSystemInfo)
    )
    |> Seq.toList

let file condition action = {
    Condition = condition
    Action = action
}

let dir condition action childDirectorysRules childFilesRules = {
    Condition = condition
    SelfAction = action
    DirectoryChildRules = DirectoryChildRules.Process(childDirectorysRules, childFilesRules)
}

let dir' condition action = {
    Condition = condition
    SelfAction = action
    DirectoryChildRules = DirectoryChildRules.Noop
}

let ignoreDirectorys = [ dir Any ]

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
            dir (Name(RegexMatch "^\d{4}$")) Noop [] []
            dir (Name(RegexMatch "^\d{4}-\d{2}$")) Noop [ dir' (Name(RegexMatch "^\d{4}-\d{2}-\d{2}-")) Noop ] []
        ] []
        dir (Name(Eq "Downloads")) Noop [] [
            file
                (Or [
                    Extension(Eq ".stl")
                    Extension(Eq ".3mf")
                ])
                (Move(SubDirectory "3d-parts"))

        ]
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
        dir (Name(Eq "Links")) Hide [] [ file (Extension(Eq ".lnk")) F.Noop ]
        dir (Name(Eq "Pictures")) Hide [
            dir' (Name(Eq "Screenpresso")) Noop
            dir (Name(Eq "Wallpapers")) Noop [] [
                file
                    (Or [
                        Extension(Eq ".png")
                        Extension(Eq ".jpg")
                    ])
                    F.Noop
            ]
            dir (Name(Eq "Camera Roll")) Noop [] []
            dir (Name(Eq "Saved Pictures")) Noop [] []
            dir (Name(Eq "Feedback")) Delete [
                dir (Name(RegexMatch @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$")) Delete [] [
                    file (Name(RegexMatch @"\.png$")) F.Delete
                ]
            ] []
            dir (Name(Eq "Zwift")) Delete [] [ file (Extension(Eq ".jpg")) F.Delete ]
            dir (Name(Eq "Screenshots")) Noop [] [ file (Extension(Eq ".png")) F.Delete ]
        ] []
        dir (Name(Eq "Music")) Hide [] []
        dir (Name(Eq "Videos")) Hide [
            dir (Name(Eq "Captures")) Noop [] [ file (Extension(Eq ".mp4")) F.Delete ]
            dir (Name(Eq "AnyDesk")) Delete [] []
        ] []
        dir (Name(Eq "Searches")) Hide [] [
            file (Extension(Eq ".search-ms")) F.Noop
            file (Extension(Eq ".searchconnector-ms")) F.Noop
        ]
        dir (Name(Eq "Saved Games")) Hide [] []
        dir (Name(Eq "ai_overlay_tmp")) Hide [] []
        dir (Name(Eq "Desktop")) Noop [] [
            file (Extension(Eq ".lnk")) F.Delete
            file (Extension(Eq ".appref-ms")) F.Delete
            file (Extension(Eq ".url")) F.Delete
        ]
        dir' (Name(Eq "Favorites")) Noop
        dir' (Name(Eq "IntelGraphicsProfiles")) Hide
        dir' (Name(Eq "Perso")) Noop
        dir (Name(Eq "Postman")) Delete [ dir (Name(Eq "files")) Delete [] [] ] []
        dir (Name(Eq "nuget")) Noop [] [ file (Extension(Eq ".nupkg")) F.Noop ]
        dir (Name(Eq "source")) Delete [ dir (Name(Eq "repos")) Delete [] [] ] []
        dir (Name(Eq "Documents")) Noop [
            dir (Name(Eq "Power BI Desktop")) Delete [ dir (Name(Eq "Custom Connectors")) Delete [] [] ] []
            dir (Name(Eq "FeedbackHub")) Delete [] []
            dir (Name(Eq "Custom Office Templates")) Delete [] []
            dir' (Name(Eq "KerialisLogs")) Noop
            dir' (Name(Eq "PICRIS")) Noop
            dir' (Name(Eq "Fiddler2")) Noop
            dir' (Name(Eq "Dell")) Noop
            dir' (Name(Eq "Zwift")) Noop
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
        ] [ file (Name(Eq "Default.rdp")) F.Hide ]
    ] [
        file (Name(Eq ".editorconfig")) F.Noop
        file
            (Or [
                Name(StartsWith ".")
                Name(StartsWith "_")
            ])
            F.Hide

        file (Name(RegexMatch @"^java_error_in_rider(64)?\.hprof$")) F.Delete
        file (Name(RegexMatch @"^jcef_\d+.log$")) F.TryDelete
        file (Extension(Eq ".mdf")) F.Delete
        file (Extension(Eq ".ldf")) F.Delete
        file (Name(StartsWith @"NTUSER.")) F.Noop
        file
            (Name(RegexMatch @"(_log)?.(ldf|mdf)$")
             <&&> LastWriteTime(OlderThan(TimeSpan.FromDays(1.))))
            F.Hide
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
