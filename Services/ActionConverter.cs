using System;
using System.Collections.Generic;
using System.Linq;
using CleanupUserProfile.ActionFactory;
using CleanupUserProfile.Actions;
using CleanupUserProfile.Config;

namespace CleanupUserProfile.Services
{
    internal class ActionConverter : IActionConverter
    {
        private readonly IEnumerable<IActionFactory<>> _actionFactories;

        public ActionConverter(
            IEnumerable<IActionFactory<>> actionFactories)
        {
            _actionFactories = actionFactories;
        }

        public IEnumerable<IAction> Convert(
            IEnumerable<GenericRule> configFiles)
        {
            return configFiles
                .Select(ConvertSingle)
                .ToList();
        }

        private IAction ConvertSingle(
            GenericRule arg)
        {
            var (name, value) = Get(arg);
            var actionFactory = _actionFactories.SingleOrDefault(a => a.ActionName == name);
            if (actionFactory == null)
            {
                throw new Exception($"Failed to determine action for {name}.");
            }

            return actionFactory.GetAction(value);
        }

        private (string Name, object Value) Get(
            GenericRule genericRule)
        {
            var props = genericRule.GetType().GetProperties();
            return (
                from p in props
                let v = p.GetValue(genericRule)
                where v != null
                select (p.Name, v)
            ).Single();
        }
    }
}