using System.Collections.Generic;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services
{
    internal interface IActionConverter
    {
        IEnumerable<IAction> Convert(
            IEnumerable<GenericRule> configFiles);
    }
}