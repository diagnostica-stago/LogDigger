using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SimpleLogger;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// Helper class regarding the InjectViewAttribute
    /// </summary>
    public static class InjectViewAttributeHelper
    {
        /// <summary>
        /// Get the InjectView decorated class with the highest overriding level from the given dictionary of {Type, InjectViewAttribute}
        /// </summary>
        public static IList<Tuple<Type, InjectViewAttribute>> GetHighestLevelInjections(IList<Tuple<Type, InjectViewAttribute>> viewFactoryAttributes)
        {
            var uniqueValues = viewFactoryAttributes.GroupBy(pair => pair.Item2.ViewModelType)
                .Select(
                    group =>
                    {
                        // get the highest priority injection
                        int maxPriority = @group.Max(pair => pair.Item2.OverridingLevel);
                        var elements = @group.Where(pair => pair.Item2.OverridingLevel == maxPriority);
                        var pairs = elements.ToList();
                        if (pairs.Count() > 1)
                        {
                            var viewModelType = pairs.First().Item2.ViewModelType;
                            var listOfViews = string.Empty;
                            foreach (var pair in pairs)
                            {
                                listOfViews += string.Format(CultureInfo.InvariantCulture, "{0}, ", pair.Item1.Name);
                            }
                            Logger.Log($"Views injected with the same priority on the same view model [{viewModelType.Name}] have been found. The last one will be used: {listOfViews}");
                        }
                        return pairs.Last();
                    })
                .ToList();
            return uniqueValues;
        }
    }
}