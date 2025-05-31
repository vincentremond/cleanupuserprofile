open System
open System.IO
open System.Text.RegularExpressions
open Pinicola.FSharp
open Pinicola.FSharp.IO
open Pinicola.FSharp.RegularExpressions
open Pinicola.FSharp.SpectreConsole
open CleanupUserProfile

let testStringRule str stringRule =
    match stringRule with
    | StartsWith prefix -> String.startsWithCurrentCultureIgnoreCase str prefix
    | RegexMatch pattern -> pattern |> Regex |> Regex.isMatch str
    | Eq value -> String.equalsCurrentCultureIgnoreCase str value

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

let rec runRoot (root: RootRules) =
    run root.Directory root.SubRules

and run (directory: DirectoryInfo) (p: Process) =
    (runDirectorys directory p.Directories) @ (runFiles directory p.Files)

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
                match directoryRule.SubRules with
                | DirectoryChildRules.Process p -> run dirInfo p
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

let notProcessedItems = Rules.all |> List.collect runRoot

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
