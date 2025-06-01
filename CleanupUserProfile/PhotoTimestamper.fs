namespace CleanupUserProfile

open System
open System.IO
open System.Reflection.PortableExecutable
open System.Text.RegularExpressions
open MetadataExtractor
open MetadataExtractor.Formats.Exif
open MetadataExtractor.Formats.FileSystem
open Pinicola.FSharp
open Pinicola.FSharp.IO
open Pinicola.FSharp.SpectreConsole

[<RequireQualifiedAccess>]
module DirectoryHelper =

    let getDirectoryByType<'T when 'T :> Directory> (metadata: Directory seq) =
        metadata
        |> Seq.choose (fun d ->
            match d with
            | :? 'T as dir -> Some dir
            | _ -> None
        )
        |> Seq.tryExactlyOne

[<RequireQualifiedAccess>]
module PhotoTimestamper =

    [<RequireQualifiedAccess>]
    type Result =
        | Prefix of DateTime
        | Substitute of DateTime * string
        | Failed

        static member fromOption f option =
            match option with
            | None -> Result.Failed
            | Some x -> f x

        static member tryPick f list =
            let rec loop =
                function
                | [] -> Failed
                | x :: xs ->
                    match f x with
                    | Failed -> loop xs
                    | result -> result

            loop list

    let tryGetFromFileName (fileName: string) =

        let regexes =
            [
                // 20190222_211533
                "(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})_(?<Hour>\d{2})(?<Minute>\d{2})(?<Second>\d{2})"
                // "2019-02-13_17-55-11_IMG-EFFECTS.jpg"
                "(?<Year>\d{4})-(?<Month>\d{2})-(?<Day>\d{2})_(?<Hour>\d{2})-(?<Minute>\d{2})-(?<Second>\d{2})"
                // 20190722-21.54.51
                "(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})-(?<Hour>\d{2})\.(?<Minute>\d{2})\.(?<Second>\d{2})"
            ]
            |> List.map Regex

        regexes
        |> Result.tryPick (fun regex ->
            let m = regex.Match(fileName)

            if m.Success then
                let year = int m.Groups.["Year"].Value
                let month = int m.Groups.["Month"].Value
                let day = int m.Groups.["Day"].Value
                let hour = int m.Groups.["Hour"].Value
                let minute = int m.Groups.["Minute"].Value
                let second = int m.Groups.["Second"].Value

                let dateTime = DateTime(year, month, day, hour, minute, second)

                Result.Substitute(dateTime, m.Value)
            else
                Result.Failed

        )

    let tryGetFromFile (file: FileInfo) =
        let fileName = file.Name
        tryGetFromFileName fileName

    let private tryGetDateTimeFromExif (file: FileInfo) =

        let getDate getDirectory tags metadata =
            let directory = metadata |> getDirectory

            match directory with
            | None -> Result.Failed
            | Some dir ->

                tags
                |> Result.tryPick (fun tag ->
                    DirectoryExtensions.TryGetDateTime(dir, tag)
                    |> Try.toOption
                    |> Result.fromOption Result.Prefix
                )

        let metadata = ImageMetadataReader.ReadMetadata(file.FullName) |> List.ofSeq

        [
            getDate DirectoryHelper.getDirectoryByType<ExifSubIfdDirectory> [
                ExifSubIfdDirectory.TagDateTimeOriginal
                ExifSubIfdDirectory.TagDateTime
                ExifSubIfdDirectory.TagDateTimeDigitized
            ]
            getDate DirectoryHelper.getDirectoryByType<FileMetadataDirectory> [ FileMetadataDirectory.TagFileModifiedDate ]
        ]
        |> Result.tryPick (fun getter -> getter metadata)

    let private rename (file: FileInfo) (newFileName: string) =
        let newFilePath = file.DirectoryName </> newFileName
        file.MoveTo(newFilePath, overwrite = false)
        newFilePath |> FileInfo

    let apply (file: FileInfo) =

        let result =
            [
                tryGetFromFile
                tryGetDateTimeFromExif
            ]
            |> Result.tryPick (fun f -> f file)

        let originalFileName = file.Name

        let newFileName =
            match result with
            | Result.Failed -> failwithf $"Failed to get date time from {file.FullName}"
            | Result.Prefix dateTime ->

                let timestamp = dateTime.ToString("yyyy-MM-dd HH-mm-ss")
                $"{timestamp} {originalFileName}"

            | Result.Substitute(dateTime, toReplace) ->

                let timestamp = dateTime.ToString("yyyy-MM-dd HH-mm-ss")

                let fileNameWithoutTimestamp =
                    originalFileName |> String.replace toReplace String.Empty

                $"{timestamp} {fileNameWithoutTimestamp}"

        AnsiConsole.markupLineInterpolated $"[blue]Renaming file[/] \"[bold white]{file.FullName}[/]\" to \"[bold white]{newFileName}[/]\""
        rename file newFileName
