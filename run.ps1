try {
    Push-Location

    Set-Location .\src\CleanupUserProfile\

    dotnet run -- --simulate
    Read-Host "Press enter to continue ?"
    dotnet run
    Read-Host "Press enter to exit"
}
finally {
    Pop-Location
}