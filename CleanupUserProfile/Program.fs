open System.Text.RegularExpressions

type NameDescriptor =
    | Exact of string
    | Fuzzy of Regex

let (!?) s = Regex s

let checkHidden d = ()
let ignore d = ()

let removeSymbolicLink d = ()
let checkEmptyDirectory d = ()
let remove d = ()
let checkEmptyDirectoryAndHide d = ()
let checkEmptyDirectoryAndRemove d = ()
let noop d = ()
let quietRemove d = ()
let checkNotHidden d = ()

let dir a n d f = ()

let cleanup desc directories files = ()

dir
    noop
    "~"
    [ checkHidden !? "^[_\.][\w\d\.-]+$"
      checkHidden "IntelGraphicsProfiles"
      checkHidden "Links"
      checkHidden "MicrosoftEdgeBackups"
      checkHidden "OpenVPN"
      ignore "AppData"
      ignore "Qa"
      removeSymbolicLink "Application Data"
      removeSymbolicLink "Cookies"
      removeSymbolicLink "Local Settings"
      removeSymbolicLink "SendTo"
      // FR
      removeSymbolicLink "Menu Démarrer"
      removeSymbolicLink "Mes documents"
      removeSymbolicLink "Modèles"
      removeSymbolicLink "Voisinage d'impression"
      removeSymbolicLink "Voisinage réseau"
      // EN
      removeSymbolicLink "My Documents"
      removeSymbolicLink "NetHood"
      removeSymbolicLink "PrintHood"
      removeSymbolicLink "Start Menu"
      removeSymbolicLink "Templates"
      ignore "OneDrive - FNAC"
      checkHidden "Favorites"
      ignore "Recent"
      checkHidden "Searches"
      ignore "repos"
      ignore "Wallpapers"
      checkEmptyDirectory "Downloads"
      dir checkHidden "source" [ checkEmptyDirectory "repos" ] []
      remove "RiderProjects"
      checkEmptyDirectoryAndHide "Music"
      checkEmptyDirectoryAndHide "3D Objects"
      checkEmptyDirectoryAndHide "Contacts"
      checkEmptyDirectoryAndHide "Saved Games"
      checkEmptyDirectoryAndRemove "dwhelper"
      dir
          noop
          "Documents"
          [ ignore "GIT"
            ignore "Mes sources de données"
            ignore "WindowsPowerShell"
            ignore "Fusion 360"
            remove "Downloads"
            remove "Custom Office Templates"
            remove "Fiddler2"
            remove "IISExpress"
            remove "Mes fichiers reçus"
            remove "Modèles Office personnalisés"
            remove "My Received Files"
            remove "My Web Sites"
            remove "Outlook Files"
            remove "SQL Server Management Studio"
            remove "Visual Studio 2015"
            remove "Visual Studio 2017"
            remove "Visual Studio 2019"
            checkEmptyDirectoryAndRemove "Audacity"
            checkEmptyDirectoryAndRemove "vcam"
            checkEmptyDirectoryAndRemove "Zoom"
            // FR
            removeSymbolicLink "Ma musique"
            removeSymbolicLink "Mes images"
            removeSymbolicLink "Mes vidéos"
            // EN
            removeSymbolicLink "My Music"
            removeSymbolicLink "My Pictures"
            removeSymbolicLink "My Videos"
            dir
                checkHidden
                "Voicemeeter"
                []
                [ remove !? "\.wav$"
                  ignore !? "\.xml$" ] ]
          [ checkHidden "Default.rdp" ]
      dir
          noop
          !? "^Google\s*Drive$"
          [ ignore "Checklist"
            ignore "Documents"
            ignore "Draft"
            ignore "Images"
            ignore "ok"
            ignore "Projets"
            ignore "TMP"
            checkHidden ".tmp.drivedownload" ]
          []
      dir
          noop
          "Pictures"
          [ checkEmptyDirectoryAndHide "Camera Roll"
            checkEmptyDirectoryAndHide "Saved Pictures"
            checkEmptyDirectoryAndHide "Screenshots"
            ignore "Screenpresso" ]
          []
      dir noop "Videos" [ checkEmptyDirectoryAndHide "Captures" ] []
      dir
          noop
          "Desktop"
          [

          ]
          [ remove !? "\.lnk$" ] ]
    [ ignore !? "^ntuser\."
      checkNotHidden ".yarnrc"
      checkHidden !? "^[_\.][\w\d\.-]+$"
      remove "coffeelint.json"
      remove "tslint.json"
      remove "Sti_Trace.log"
      remove !? "^\.v8flags\..+\.json"
      remove !? "^tmp[\w\d\.-]+.pem" ]

dir
    noop
    @"D:\DAT\"
    [ dir noop "MergeRequestReview" [ quietRemove !? ".*" ] []
      dir noop "FIXObs" [ quietRemove !? ".*" ] [] ]
    []

dir noop @"D:\TMP\" [ dir noop !? "^\d{4}$") [ ignore (Regex "^\d{4}\.\d{2}$" ] [] ] []

dir noop @"G:\My Drive\" [ ignore "BAK"; ignore "BOD" ] [ ignore "editorconfig.gsheet" ]
