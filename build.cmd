@ECHO OFF

dotnet tool restore
dotnet build -- %*

add-to-path .\CleanupUserProfile\bin\Debug\
