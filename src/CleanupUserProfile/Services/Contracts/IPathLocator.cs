﻿using System.IO;

namespace CleanupUserProfile.Services.Contracts
{
    internal interface IPathLocator
    {
        DirectoryInfo Locate(string directoryName);
    }
}
