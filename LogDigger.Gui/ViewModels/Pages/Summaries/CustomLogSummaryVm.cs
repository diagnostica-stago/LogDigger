using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.LogEntries;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public class CustomLogSummaryVm : ALogSummaryVm
    {
        private readonly string[] _matchValues;
        private string _details;
        private string _typeName;
        private string _sourceModule;
        public CustomLogTemplate Template { get; }

        public CustomLogSummaryVm(LogEntry sourceEntry, CustomLogTemplate template, IEnumerable<string> matchValues) 
            : base(sourceEntry)
        {
            _sourceModule = SourceEntry.GetModule();
            _matchValues = matchValues?.ToArray() ?? Array.Empty<string>();
            Template = template;
            var matchCollec = Regex.Match(Template.Details, @".*(\[(.*)\])");
            var details = template.Details;
            details = Preformat(details);
            if (matchCollec.Success)
            {
                var collecString = matchCollec.Groups[1].Value;
                _details = TryFormat(details.Replace(collecString, "?"), _matchValues);
            }
            else
            {
                _details = TryFormat(details, _matchValues);
            }
            _typeName = Template.TypeName?.Replace("{m}", _sourceModule);
        }

        private string Preformat(string details)
        {
            return details.Replace("{m}", _sourceModule);
        }

        public override string Details => _details;

        public override string TypeColor => Template.TypeColor;

        public override string TypeName => _typeName;

        public override LogType LogType => LogType.Unknown;

        public override void LookForExtraInformation(IEnumerable<LogEntry> entries)
        {
            if (!(Template.Subpatterns?.Any() ?? false))
            {
                return;
            }
            var matchCollec = Regex.Match(Template.Details, @".*(\[(.*)\])");
            if (!matchCollec.Success)
            {
                return;
            }
            var collecString = matchCollec.Groups[1].Value;
            var collecFormat = matchCollec.Groups[2].Value;
            // TODO: for now, we are supporting only one subpattern
            var subpattern = Template.Subpatterns.First();
            var matchValues = new List<string>();
            foreach (var entry in entries)
            {
                var match = Regex.Match(entry.Content, subpattern);
                if (match.Success)
                {
                    var values = match.Groups.Cast<Group>().Skip(1).Select(x => x.Value).ToArray();
                    var matchValue = TryFormat(collecFormat, values);
                    matchValues.Add(matchValue);
                }
            }
            var newDetails = Template.Details.Replace(collecString, string.Join(" ; ", matchValues));
            newDetails = Preformat(newDetails);
            _details = TryFormat(newDetails, _matchValues);
            this.RaisePropertyChanged(nameof(Details));
        }

        public string TryFormat(string format, params object[] args)
        {
            try
            {
                return string.Format(format, args);
            }
            catch (Exception)
            {
                return "Format error";
            }
        }

        public static CustomLogSummaryVm BuildFromTemplate(LogEntry entry, CustomLogTemplate template, Match match)
        {
            return new CustomLogSummaryVm(entry, template, match?.Groups.Cast<Group>().Skip(1).Select(x => x.Value));
        }
    }
}