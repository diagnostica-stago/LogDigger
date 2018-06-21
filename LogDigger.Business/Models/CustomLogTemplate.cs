using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LogDigger.Business.Models
{
    public class CustomLogTemplate
    {
        public bool IsException { get; set; }
        public string Pattern { get; set; }
        public string Module { get; set; }
        public string TypeColor { get; set; }
        public string TypeName { get; set; }
        public string Details { get; set; }
        public List<string> Subpatterns { get; set; }

        public TemplateMatch MatchEntry(LogEntry entry)
        {
            var logContent = entry.Content;
            var module = entry.Parent.Module;
            var matchIsException = IsException == entry.IsException;
            if (logContent == null || !matchIsException || !string.IsNullOrEmpty(Module) && Module != module)
            {
                return new TemplateMatch(false);
            }
            if (!string.IsNullOrEmpty(Pattern))
            {
                var matchTemplate = Regex.Match(logContent, Pattern);
                if (matchTemplate.Success)
                {
                    return new TemplateMatch(true, matchTemplate);
                }
            }
            else
            {
                return new TemplateMatch(true);
            }
            return new TemplateMatch(false);
        }
    }
}