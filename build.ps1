$ErrorActionPreference = "Stop"

dotnet tool restore
dotnet build

AddToPath .\CleanupUserProfile\bin\Debug\
