open System
open System.IO
open System.Text.RegularExpressions

type Rule = MetaAction * Condition

and MetaAction =
    // TODO VRM remove MetaAction
    | Do of Action
    | SubFolder of SubFolder
    | SubFolderWithAction of SubFolder * Action

and SubFolder = {
    FolderRules: Rule list
    FileRules: Rule list
}

and Action =
    | Hide
    | Ignore
    | Unlink
    | Delete

and StringRule =
    | StartsWith of string
    | Match of string
    | Eq of string

and Condition =
    | Name of StringRule
    | Extension of StringRule
    | IsSymLink
    | IsHidden
    | IsReadOnly
    | IsSystem
    | And of Condition list
    | Or of Condition list
    | Not of Condition

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
    | Ignore -> ()
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

let rec genericProcess (type_: string) (items: FileSystemInfo list) (rules: (MetaAction * Condition) list) : unit =
    let notProcessedItems =
        items
        |> List.fold
            (fun notProcessedList item ->
                let metaAction =
                    rules
                    |> List.tryPick (fun (metaAction, condition) ->
                        if testCondition condition item then
                            Some metaAction
                        else
                            None
                    )

                match metaAction, item with
                | Some(Do action), _ ->
                    applyAction action item
                    notProcessedList
                | Some(SubFolder subFolder), (:? DirectoryInfo as dir) ->
                    processFolder dir subFolder.FolderRules subFolder.FileRules
                    notProcessedList
                | Some(SubFolder _), _ -> failwith "Cannot process SubFolder on something that is not a DirectoryInfo"
                | Some(SubFolderWithAction(subFolder, action)), (:? DirectoryInfo as dir) ->
                    processFolder dir subFolder.FolderRules subFolder.FileRules
                    applyAction action item
                    notProcessedList
                | Some(SubFolderWithAction _), _ -> failwith "Cannot process SubFolder on something that is not a DirectoryInfo"
                | None, _ -> item :: notProcessedList
            )
            []

    for item in notProcessedItems |> List.rev do
        printfn $"What to do with : {type_} {item}"

and processFolder (folder: DirectoryInfo) folderRules fileRules =
    genericProcess "DIR " (folder.GetDirectories() |> Seq.cast<FileSystemInfo> |> Seq.toList) folderRules
    genericProcess "FILE" (folder.GetFiles() |> Seq.cast<FileSystemInfo> |> Seq.toList) fileRules

let filesToIgnore = [
    Do(Ignore),

    And [
        Or [
            Name(Eq @"desktop.ini")
            Name(Eq @"Thumbs.db")
        ]
        IsHidden
        IsSystem
    ]
]

let emptyFolder name =
    (SubFolder {
        FolderRules = []
        FileRules = filesToIgnore
     },
     Name(Eq name))

let emptyFolderWithAction name action =
    (SubFolderWithAction(
        {
            FolderRules = []
            FileRules = filesToIgnore
        },
        action
     ),
     Name(Eq name))

let subFolder name foldersRules filesRules =
    (SubFolder {
        FolderRules = foldersRules
        FileRules = filesToIgnore @ filesRules
     },
     Name(Eq name))

let subFolderWithAction name action foldersRules filesRules =
    (SubFolderWithAction(
        {
            FolderRules = foldersRules
            FileRules = filesToIgnore @ filesRules
        },
        action
     ),
     Name(Eq name))

let subFolderWithAction' condition action foldersRules filesRules =
    (SubFolderWithAction(
        {
            FolderRules = foldersRules
            FileRules = filesToIgnore @ filesRules
        },
        action
     ),
     condition)

processFolder userProfile [
    Do(Hide),
    Or [
        Name(StartsWith("."))
        Name(StartsWith("_"))
    ]
    Do(Ignore), Name(Eq @"AppData")
    Do(Ignore), Name(Eq @"Apps")
    Do(Ignore), Name(Eq @"Data")
    Do(Ignore), Name(Eq @"repos")
    Do(Ignore), Name(Eq @"tmp")
    Do(Ignore), Name(Eq @"OneDrive")
    Do(Ignore), Name(StartsWith("OneDrive -"))
    emptyFolder "Downloads"
    Do(Unlink),
    And [
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
    ]
    subFolderWithAction @"dotTraceSnapshots" Delete [
        Do(Delete), Name(Eq "Temp")
        subFolderWithAction' (Name(StartsWith("Unit Tests "))) Delete [] [ Do(Delete), Name(Match @"\.tmp(\.\d+)?$") ]
    ] []
    emptyFolderWithAction @"Contacts" Hide
    subFolderWithAction @"Links" Hide [] [ Do(Ignore), Extension(Eq ".lnk") ]
    subFolderWithAction @"Pictures" Hide [
        Do(Ignore), Name(Eq @"Screenpresso")
        Do(Ignore), Name(Eq "Wallpapers")
        emptyFolder "Camera Roll"
        emptyFolder "Saved Pictures"
        subFolderWithAction "Feedback" Delete [
            subFolderWithAction' (Name(Match @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$")) Delete [] [
                Do(Delete), Name(Match @"\.png$")
            ]
        ] []

    ] []
    emptyFolderWithAction @"Music" Hide
    subFolderWithAction @"Videos" Hide [
        emptyFolder "Captures"
        emptyFolderWithAction "AnyDesk" Delete
    ] []
    subFolderWithAction @"Searches" Hide [] [
        Do(Ignore), Extension(Eq ".search-ms")
        Do(Ignore), Extension(Eq ".searchconnector-ms")
    ]
    emptyFolderWithAction @"Saved Games" Hide
    emptyFolderWithAction @"ai_overlay_tmp" Hide
    subFolder "Desktop" [] [
        Do(Delete), Extension(Eq ".lnk")
        Do(Delete), Extension(Eq ".url")
    ]
    subFolderWithAction "Postman" Delete [ emptyFolderWithAction "files" Delete ] []
    Do(Hide), Name(Eq @"IntelGraphicsProfiles")
    Do(Ignore), Name(Eq @"Favorites")
    Do(Ignore), Name(Eq @"Perso")
    subFolder "nuget" [] [ Do(Ignore), Extension(Eq ".nupkg") ]
    subFolderWithAction "source" Delete [ Do(Delete), Name(Eq "repos") ] []
    subFolder "Documents" [
        emptyFolderWithAction "Custom Office Templates" Delete
        Do(Ignore), Name(Eq @"Fiddler2")
        Do(Ignore), Name(Eq @"Dell")
        subFolderWithAction "PowerToys" Delete [ emptyFolderWithAction "Backup" Delete ] []
        subFolder "Screenpresso" [
            subFolderWithAction "Originals" Delete [] [ Do(Delete), Extension(Eq ".presso") ]
            subFolder "Thumbnails" [] [ Do(Ignore), Extension(Eq ".png") ]

        ] [ Do(Ignore), Extension(Eq ".png") ]

        Do(Unlink),
        Or [
            Name(Eq @"Ma musique")
            Name(Eq @"Mes images")
            Name(Eq @"Mes vidéos")
            Name(Eq @"My Music")
            Name(Eq @"My Pictures")
            Name(Eq @"My Videos")
        ]
        Do(Ignore),
        Or [
            Name(Eq "IISExpress")
            Name(Eq "PowerShell")
            Name(Eq "WindowsPowerShell")
            Name(Eq "My Web Sites")
            Name(Eq "Visual Studio 2017")
            Name(Eq "Visual Studio 2022")
        ]
        emptyFolder "Fichiers Outlook"
        emptyFolder "Zoom"
    ] []

] [
    Do(Ignore), Name(Eq ".editorconfig")
    Do(Hide), Name(StartsWith ".")
    Do(Hide), Name(StartsWith "_")
    Do(Ignore), Name(StartsWith @"NTUSER.")
    Do(Ignore), Name(Eq @"desktop.ini")
    Do(Hide), Name(Match @"^AzureStorageEmulatorDb\d+(_log)?.(ldf|mdf)$")
]
