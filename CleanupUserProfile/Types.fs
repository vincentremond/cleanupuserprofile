[<AutoOpen>]
module CleanupUserProfile.Types

open System
open System.IO

type RootRules = {
    Directory: DirectoryInfo
    SubRules: Process
} with

    static member init root dirs files = {
        Directory = root
        SubRules = {
            Directories = dirs
            Files = files
        }
    }

and FileRule = {
    Condition: Condition
    Action: FileAction
} with

    static member init condition action = {
        Condition = condition
        Action = action
    }

and DirectoryRule = {
    Condition: Condition
    SelfAction: DirectoryAction
    SubRules: DirectoryChildRules
} with

    static member initNoop condition action = {
        Condition = condition
        SelfAction = action
        SubRules = DirectoryChildRules.Noop
    }

    static member init condition action dirs files = {
        Condition = condition
        SelfAction = action
        SubRules = DirectoryChildRules.initProcess dirs files
    }

and DirectoryChildRules =
    | Noop
    | Process of Process

    static member initProcess directories files =
        Process {
            Directories = directories
            Files = files
        }

and Process = {
    Directories: DirectoryRule list
    Files: FileRule list
} with

    static member init directories files = {
        Directories = directories
        Files = files
    }

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

and MoveDestination =
    | SubDirectory of string
    | Directory of DirectoryInfo

    static member initSubDirectory subDir = SubDirectory subDir

    static member initDirectory(dir: string) =
        if not (Path.IsPathRooted(dir)) then
            failwith "Directory path must be absolute"

        dir |> DirectoryInfo |> Directory

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
    | StartsWithAny of string list
    | RegexMatch of string
    | Eq of string
    | EqAny of string list

and DateTimeCondition =
    | OlderThan of TimeSpan
    | NewerThan of TimeSpan
    | Before of DateTime
    | After of DateTime
