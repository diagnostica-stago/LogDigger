using System.Collections.Generic;
using LogDigger.Business.Models;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public static class LogSummaryFactory
    {
        public static ALogSummaryVm BuildLogSummary(IReadOnlyList<CustomLogTemplate> templates, LogEntry entry)
        {
            foreach (var template in templates)
            {
                var match = template.MatchEntry(entry);
                if (match.Success)
                {
                    return CustomLogSummaryVm.BuildFromTemplate(entry, template, match.PatternMatch);
                }
            }
            return null;
        }
    }
}