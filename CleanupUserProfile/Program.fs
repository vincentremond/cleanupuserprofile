open System
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

and StringRule =
    | StartsWith of string
    | Match of string
    | Eq of string

and MatchRule =
    | Name of StringRule
    | Extension of StringRule
    | IsSymLink
    | IsHidden
    | IsReadOnly
    | IsSystem

let (|&|) a b = And(a, b)
let (|+|) a b = Or(a, b)

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

let testRule rule (item: FileSystemInfo) =
    match rule with
    | Name stringRule -> testStringRule item.Name stringRule
    | IsSymLink -> item.Attributes.HasFlag(FileAttributes.ReparsePoint)
    | IsHidden -> item.Attributes.HasFlag(FileAttributes.Hidden)
    | IsReadOnly -> item.Attributes.HasFlag(FileAttributes.ReadOnly)
    | IsSystem -> item.Attributes.HasFlag(FileAttributes.System)
    | Extension stringRule -> testStringRule item.Extension stringRule

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
    (When(Name(Eq @"desktop.ini")) |+| When(Name(Eq @"Thumbs.db")))
    |&| When(IsHidden)
    |&| When(IsSystem)
]

let emptyFolder name =
    (SubFolder {
        FolderRules = []
        FileRules = filesToIgnore
     },
     When(Name(Eq name)))

let emptyFolderWithAction name action =
    (SubFolderWithAction(
        {
            FolderRules = []
            FileRules = filesToIgnore
        },
        action
     ),
     When(Name(Eq name)))

let subFolder name foldersRules filesRules =
    (SubFolder {
        FolderRules = foldersRules
        FileRules = filesToIgnore @ filesRules
     },
     When(Name(Eq name)))

let subFolderWithAction name action foldersRules filesRules =
    (SubFolderWithAction(
        {
            FolderRules = foldersRules
            FileRules = filesToIgnore @ filesRules
        },
        action
     ),
     When(Name(Eq name)))

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
    Do(Hide), When(Name(StartsWith("."))) |+| When(Name(StartsWith("_")))
    Do(Ignore), When(Name(Eq @"AppData"))
    Do(Ignore), When(Name(Eq @"Apps"))
    Do(Ignore), When(Name(Eq @"Data"))
    Do(Ignore), When(Name(Eq @"repos"))
    Do(Ignore), When(Name(Eq @"tmp"))
    Do(Ignore), When(Name(Eq @"OneDrive"))
    Do(Ignore), When(Name(StartsWith("OneDrive -")))
    emptyFolder "Downloads"
    Do(Unlink),
    (When(IsSymLink)
     |&| (When(Name(Eq @"Application Data"))
          |+| When(Name(Eq @"Cookies"))
          |+| When(Name(Eq @"Local Settings"))
          |+| When(Name(Eq @"Menu Démarrer"))
          |+| When(Name(Eq @"Mes documents"))
          |+| When(Name(Eq @"Modèles"))
          |+| When(Name(Eq @"My Documents"))
          |+| When(Name(Eq @"NetHood"))
          |+| When(Name(Eq @"PrintHood"))
          |+| When(Name(Eq @"Recent"))
          |+| When(Name(Eq @"SendTo"))
          |+| When(Name(Eq @"Start Menu"))
          |+| When(Name(Eq @"Templates"))
          |+| When(Name(Eq @"Voisinage d'impression"))
          |+| When(Name(Eq @"Voisinage réseau"))))

    subFolderWithAction @"dotTraceSnapshots" Delete [
        Do(Delete), When(Name(Eq "Temp"))
        subFolderWithAction' (When(Name(StartsWith("Unit Tests ")))) Delete [] [ Do(Delete), When(Name(Match @"\.tmp(\.\d+)?$")) ]
    ] []
    emptyFolderWithAction @"Contacts" Hide
    subFolderWithAction @"Links" Hide [] [ Do(Ignore), When(Extension(Eq ".lnk")) ]
    subFolderWithAction @"Pictures" Hide [
        Do(Ignore), When(Name(Eq @"Screenpresso"))
        Do(Ignore), When(Name(Eq "Wallpapers"))
        emptyFolder "Camera Roll"
        emptyFolder "Saved Pictures"
        subFolderWithAction "Feedback" Delete [
            subFolderWithAction' (When(Name(Match @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$"))) Delete [] [
                Do(Delete), When(Name(Match @"\.png$"))
            ]
        ] []

    ] []
    emptyFolderWithAction @"Music" Hide
    subFolderWithAction @"Videos" Hide [
        emptyFolder "Captures"
        emptyFolderWithAction "AnyDesk" Delete
    ] []
    subFolderWithAction @"Searches" Hide [] [
        Do(Ignore), When(Extension(Eq ".search-ms"))
        Do(Ignore), When(Extension(Eq ".searchconnector-ms"))
    ]
    emptyFolderWithAction @"Saved Games" Hide
    emptyFolderWithAction @"ai_overlay_tmp" Hide
    subFolder "Desktop" [] [
        Do(Delete), When(Extension(Eq ".lnk"))
        Do(Delete), When(Extension(Eq ".url"))
    ]
    subFolderWithAction "Postman" Delete [ emptyFolderWithAction "files" Delete ] []
    Do(Hide), When(Name(Eq @"IntelGraphicsProfiles"))
    Do(Ignore), When(Name(Eq @"Favorites"))
    Do(Ignore), When(Name(Eq @"Perso"))
    subFolder "nuget" [] [ Do(Ignore), When(Extension(Eq ".nupkg")) ]
    subFolderWithAction "source" Delete [ Do(Delete), When(Name(Eq "repos")) ] []
    subFolder "Documents" [
        emptyFolderWithAction "Custom Office Templates" Delete
        Do(Ignore), When(Name(Eq @"Fiddler2"))
        Do(Ignore), When(Name(Eq @"Dell"))
        subFolderWithAction "PowerToys" Delete [ emptyFolderWithAction "Backup" Delete ] []
        subFolder "Screenpresso" [
            subFolderWithAction "Originals" Delete [] [ Do(Delete), When(Extension(Eq ".presso")) ]
            subFolder "Thumbnails" [] [ Do(Ignore), When(Extension(Eq ".png")) ]

        ] [ Do(Ignore), When(Extension(Eq ".png")) ]

        Do(Unlink),
        (When(Name(Eq @"Ma musique"))
         |+| When(Name(Eq @"Mes images"))
         |+| When(Name(Eq @"Mes vidéos"))
         |+| When(Name(Eq @"My Music"))
         |+| When(Name(Eq @"My Pictures"))
         |+| When(Name(Eq @"My Videos")))
        Do(Ignore),
        (When(Name(Eq "IISExpress"))
         |+| When(Name(Eq "PowerShell"))
         |+| When(Name(Eq "WindowsPowerShell"))
         |+| When(Name(Eq "My Web Sites"))
         |+| When(Name(Eq "Visual Studio 2017"))
         |+| When(Name(Eq "Visual Studio 2022")))
        emptyFolder "Fichiers Outlook"
        emptyFolder "Zoom"
    ] []

] [
    Do(Ignore), When(Name(Eq ".editorconfig"))
    Do(Hide), When(Name(StartsWith "."))
    Do(Hide), When(Name(StartsWith "_"))
    Do(Ignore), When(Name(StartsWith @"NTUSER."))
    Do(Ignore), When(Name(Eq @"desktop.ini"))
    Do(Hide), When(Name(Match @"^AzureStorageEmulatorDb\d+(_log)?.(ldf|mdf)$"))
]
