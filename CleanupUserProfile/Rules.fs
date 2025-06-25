namespace CleanupUserProfile

open System
open System.IO

module SpecialDirectories =
    let userProfile =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        |> DirectoryInfo

    let desktop =
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) |> DirectoryInfo

module Rules =

    let userProfile =
        RootRules.init SpecialDirectories.userProfile [
            DR.initNoop
                (Or [
                    Name(StartsWith ".")
                    Name(StartsWith "_")
                ])
                Hide
            DR.initNoop
                (Or [
                    Name(Eq "AppData")
                    Name(Eq "Apps")
                    Name(Eq "Data")
                    Name(Eq "repos")
                    Name(Eq "OneDrive")
                    Name(StartsWith "OneDrive -")
                ])
                Noop
            DR.init (Name(Eq "TMP")) Noop [
                DR.init (Name(RegexMatch "^\d{4}$")) Noop [] []
                DR.init (Name(RegexMatch "^\d{4}-\d{2}$")) Noop [
                    DR.initNoop (Name(RegexMatch "^\d{4}-\d{2}-\d{2}-")) Noop
                ] []
            ] []
            DR.init (Name(Eq "Downloads")) Noop [] [
                FR.init
                    (Or [
                        Extension(Eq ".stl")
                        Extension(Eq ".3mf")
                    ])
                    (Move(SubDirectory "3d-parts"))

            ]
            DR.initNoop
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
            DR.initNoop (Name(Eq "dotTraceSnapshots")) DeleteRecursive
            DR.initNoop (Name(Eq "RiderSnapshots")) DeleteRecursive
            DR.init (Name(Eq "Contacts")) Hide [] []
            DR.init (Name(Eq "Links")) Hide [] [ FileRule.init (Extension(Eq ".lnk")) F.Noop ]
            DR.init (Name(Eq "Pictures")) Hide [
                DR.initNoop (Name(Eq "Screenpresso")) Noop
                DR.init (Name(Eq "Wallpapers")) Noop [] [
                    FR.init
                        (Or [
                            Extension(Eq ".png")
                            Extension(Eq ".jpg")
                        ])
                        F.Noop
                ]
                DR.init (Name(Eq "Camera Roll")) Noop [] []
                DR.init (Name(Eq "Saved Pictures")) Noop [] []
                DR.init (Name(Eq "Feedback")) Delete [
                    DR.init
                        (Name(RegexMatch @"^\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$"))
                        Delete [] [ FR.init (Name(RegexMatch @"\.png$")) F.Delete ]
                ] []
                DR.init (Name(Eq "Zwift")) Delete [] [ FR.init (Extension(Eq ".jpg")) F.Delete ]
                DR.init (Name(Eq "Screenshots")) Noop [] [ FR.init (Extension(Eq ".png")) F.Delete ]
            ] []
            DR.init (Name(Eq "Music")) Hide [] []
            DR.init (Name(Eq "Videos")) Hide [
                DR.init (Name(Eq "Captures")) Noop [] [ FR.init (Extension(Eq ".mp4")) F.Delete ]
                DR.init (Name(Eq "AnyDesk")) Delete [] []
            ] []
            DR.init (Name(Eq "Searches")) Hide [] [
                FR.init (Extension(Eq ".search-ms")) F.Noop
                FR.init (Extension(Eq ".searchconnector-ms")) F.Noop
            ]
            DR.init (Name(Eq "Saved Games")) Hide [] []
            DR.init (Name(Eq "ai_overlay_tmp")) Hide [] []
            DR.init (Name(Eq "Desktop")) Noop [] [
                FR.init (Extension(Eq ".lnk")) F.Delete
                FR.init (Extension(Eq ".appref-ms")) F.Delete
                FR.init (Extension(Eq ".url")) F.Delete
            ]
            DR.initNoop (Name(Eq "Favorites")) Noop
            DR.initNoop (Name(Eq "MMSOFT Design")) Noop
            DR.initNoop (Name(Eq "IntelGraphicsProfiles")) Hide
            DR.initNoop (Name(Eq "Perso")) Noop
            DR.init (Name(Eq "Postman")) Delete [ DR.init (Name(Eq "files")) Delete [] [] ] []
            DR.init (Name(Eq "nuget")) Noop [] [ FR.init (Extension(Eq ".nupkg")) F.Noop ]
            DR.init (Name(Eq "source")) Delete [ DR.init (Name(Eq "repos")) Delete [] [] ] []
            DR.init (Name(Eq "Documents")) Noop [
                DR.init (Name(Eq "Power BI Desktop")) Delete [ DR.init (Name(Eq "Custom Connectors")) Delete [] [] ] []
                DR.init (Name(Eq "FeedbackHub")) Delete [] []
                DR.init (Name(Eq "Custom Office Templates")) Delete [] []
                DR.initNoop (Name(Eq "KerialisLogs")) Noop
                DR.initNoop (Name(Eq "PICRIS")) Noop
                DR.initNoop (Name(Eq "Fiddler2")) Noop
                DR.initNoop (Name(Eq "Dell")) Noop
                DR.initNoop (Name(Eq "Zwift")) Noop
                DR.init (Name(Eq "PowerToys")) Delete [ DR.init (Name(Eq "Backup")) Delete [] [] ] []
                DR.initNoop
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
                DR.initNoop
                    (Or [
                        Name(Eq "IISExpress")
                        Name(Eq "PowerShell")
                        Name(Eq "WindowsPowerShell")
                    ])
                    Noop
                DR.initNoop
                    (Or [
                        Name(Eq "My Web Sites")
                        Name(Eq "Visual Studio 2017")
                        Name(Eq "Visual Studio 2022")
                        Name(RegexMatch "^SQL Server Management Studio( \d+)?$")
                        Name(Eq "Modèles Office personnalisés")
                    ])
                    ContainsNoFiles
                DR.init (Name(Eq "Fichiers Outlook")) Noop [] []
                DR.init (Name(Eq "Zoom")) Noop [] []
            ] [ FR.init (Name(Eq "Default.rdp")) F.Hide ]
        ] [
            FR.init (Name(Eq ".editorconfig")) F.Noop
            FR.init
                (Name(
                    StartsWithAny [
                        "."
                        "_"
                    ]
                 )
                 <&&> Not(Name(EqAny [ ".editorconfig" ])))
                F.Hide

            FR.init (Name(RegexMatch @"^java_error_in_rider(64)?\.hprof$")) F.Delete
            FR.init (Name(RegexMatch @"^jcef_\d+.log$")) F.TryDelete
            FR.init (Name(StartsWith @"NTUSER.")) F.Noop
            FR.init
                (Name(RegexMatch @"(_log)?.(ldf|mdf)$"))
                F.Hide
        ]

    let googleDrive =
        RootRules.init (DirectoryInfo(@"G:\My Drive")) [
            // DR.init (Name(Eq "_ScanSnap")) ContainsNoFiles [] []
            // DR.init (Name(Eq "CCleaner")) ContainsNoFiles [
            //     DR.init (Name(Eq "Android")) ContainsNoFiles [
            //         DR.init (Name(Eq "media")) Delete [
            //             DR.init (Name(Eq "com.whatsapp")) Delete [
            //                 DR.init (Name(Eq "WhatsApp")) Delete [ DR.init (Name(Eq "Media")) Delete [ DR.init Any Delete [] [] ] [] ] []
            //             ] []
            //         ] []
            //     ] []
            // ] []
            DR.init (Name(Eq "Google Photos")) ContainsNoFiles [
                DR.init (Name(RegexMatch "^\d+$")) ContainsNoFiles [
                    DR.init (Name(RegexMatch "^\d+$")) ContainsNoFiles [] [
                        FR.init
                            (Extension(
                                EqAny [
                                    ".jpg"
                                    ".mp4"
                                ]
                            ))
                            (F.Multiple [
                                F.TimestampPhoto
                                F.Move(MoveDestination.initDirectory @"G:\My Drive\Photos\_misc")
                            ])
                    ]
                ] []

            ] []

        ] [ FR.init (Name(Eq "backup.ps1")) F.Noop ]

    let all = [
        userProfile
        googleDrive
    ]
