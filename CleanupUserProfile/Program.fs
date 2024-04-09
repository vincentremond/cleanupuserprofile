﻿open System
open System.IO
open System.Text.RegularExpressions

type Rule = MetaAction * Condition

and MetaAction =
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

and Condition =
    | When of MatchRule
    | And of Condition * Condition
    | Or of Condition * Condition
    | Not of Condition

and MatchRule =
    | Name of NameRules
    | Extension of string
    | IsSymLink
    | IsHidden
    | IsReadOnly
    | IsSystem

and NameRules =
    | StartsWith of string
    | StartsWithCaseSensitive of string
    | Match of string
    | Eq of string
    | EqCaseSensitive of string

let (|&|) a b = And(a, b)
let (|+|) a b = Or(a, b)

let (</>) a b = Path.Combine(a, b)

[<AutoOpen>]
module ShortHands =
    let nameStartsWith prefix = When(Name(StartsWith prefix))
    let nameEquals value = When(Name(Eq value))
    let nameMatch pattern = When(Name(Match pattern))
    let extensionEquals value = When(Extension value)
    let isSymLink = When(IsSymLink)
    let isHidden = When(IsHidden)
    let isReadOnly = When(IsReadOnly)
    let isSystem = When(IsSystem)
    let ignore = Do(Ignore)
    let delete = Do(Delete)
    let hide = Do(Hide)
    let unlink = Do(Unlink)

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

let testRule rule (item: FileSystemInfo) =
    match rule with
    | Name condition ->
        match condition with
        | StartsWith prefix -> String.startsWith item.Name prefix
        | StartsWithCaseSensitive prefix -> String.startsWithCaseSensitive item.Name prefix
        | Match pattern -> pattern |> Regex |> Regex.isMatch item.Name
        | Eq value -> String.equals item.Name value
        | EqCaseSensitive value -> String.equalsCaseSensitive item.Name value
    | IsSymLink -> item.Attributes.HasFlag(FileAttributes.ReparsePoint)
    | IsHidden -> item.Attributes.HasFlag(FileAttributes.Hidden)
    | IsReadOnly -> item.Attributes.HasFlag(FileAttributes.ReadOnly)
    | IsSystem -> item.Attributes.HasFlag(FileAttributes.System)
    | Extension extension -> String.equals item.Extension extension

let rec testCondition (condition: Condition) (item: FileSystemInfo) =
    match condition with
    | When rule -> testRule rule item
    | And(a, b) -> testCondition a item && testCondition b item
    | Or(a, b) -> testCondition a item || testCondition b item
    | Not a -> not <| testCondition a item

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
                | Some(SubFolderWithAction _), _ ->
                    failwith "Cannot process SubFolder on something that is not a DirectoryInfo"
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
    (nameEquals @"desktop.ini" |+| nameEquals @"Thumbs.db")
    |&| isHidden
    |&| isSystem
]

let emptyFolder name =
    (SubFolder {
        FolderRules = []
        FileRules = filesToIgnore
     },
     nameEquals name)

let emptyFolderWithAction name action =
    (SubFolderWithAction(
        {
            FolderRules = []
            FileRules = filesToIgnore
        },
        action
     ),
     nameEquals name)

let subFolder name foldersRules filesRules =
    (SubFolder {
        FolderRules = foldersRules
        FileRules = filesToIgnore @ filesRules
     },
     nameEquals name)

let subFolderWithAction name action foldersRules filesRules =
    (SubFolderWithAction(
        {
            FolderRules = foldersRules
            FileRules = filesToIgnore @ filesRules
        },
        action
     ),
     nameEquals name)

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
    hide, (nameStartsWith ".") |+| (nameStartsWith @"_")
    ignore, (nameEquals @"AppData")
    unlink,
    (isSymLink
     |&| (nameEquals @"Application Data"
          |+| nameEquals @"Local Settings"
          |+| nameEquals @"Modèles"
          |+| nameEquals @"Menu Démarrer"
          |+| nameEquals @"Mes documents"
          |+| nameEquals @"Voisinage d'impression"
          |+| nameEquals @"SendTo"
          |+| nameEquals @"Recent"
          |+| nameEquals @"Voisinage réseau"
          |+| nameEquals "Cookies"))

    subFolderWithAction @"dotTraceSnapshots" Delete [
        delete, nameEquals "Temp"
        subFolderWithAction' (nameStartsWith "Unit Tests ") Delete [] [ delete, nameMatch @"\.tmp(\.\d+)?$" ]
    ] []
    emptyFolderWithAction @"Contacts" Hide
    subFolderWithAction @"Pictures" Hide [ ignore, nameEquals "Screenpresso" ] []
    emptyFolderWithAction @"Music" Hide
    subFolderWithAction @"Videos" Hide [
        emptyFolder "Captures"
        emptyFolderWithAction "AnyDesk" Delete
    ] []
    subFolderWithAction @"Searches" Hide [] [ ignore, extensionEquals ".search-ms" ]
    emptyFolderWithAction @"Saved Games" Hide
    subFolder "Desktop" [] [ Do(Delete), extensionEquals ".lnk" ]
    subFolderWithAction "Postman" Delete [ emptyFolderWithAction "files" Delete ] []
    hide, nameEquals @"IntelGraphicsProfiles"
    ignore, nameEquals @"Favorites"
    ignore, nameEquals @"Perso"
    subFolder "nuget" [] [ ignore, extensionEquals @".nupkg" ]
    subFolder "Documents" [
        emptyFolderWithAction "Custom Office Templates" Delete
        ignore, nameEquals @"Fiddler2"
        subFolderWithAction "PowerToys" Delete [ emptyFolderWithAction "Backup" Delete ] []
        subFolder "Screenpresso" [
            subFolderWithAction "Originals" Delete [] [ delete, extensionEquals ".presso" ]
            subFolder "Thumbnails" [] [ ignore, extensionEquals ".png" ]

        ] [ ignore, extensionEquals ".png" ]

        unlink,
        (nameEquals @"Ma musique"
         |+| nameEquals @"Mes images"
         |+| nameEquals @"Mes vidéos")
        ignore,
        (nameEquals "IISExpress"
         |+| nameEquals "PowerShell"
         |+| nameEquals "WindowsPowerShell"
         |+| nameEquals "My Web Sites"
         |+| nameEquals "Visual Studio 2017"
         |+| nameEquals "Visual Studio 2022")
        emptyFolder "Fichiers Outlook"

    ] [ ignore, nameEquals "Default.rdp" ]

] [
    Do(Hide), When(Name(StartsWith "."))
    Do(Hide), When(Name(StartsWith "_"))
    Do(Ignore), When(Name(StartsWith @"NTUSER."))
    Do(Hide), When(Name(Match @"^AzureStorageEmulatorDb\d+(_log)?.(ldf|mdf)$"))
]
