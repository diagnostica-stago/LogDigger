using System.Text.RegularExpressions;

namespace LogDigger.Business.Models
{
    public struct TemplateMatch
    {
        public TemplateMatch(bool success, Match patternMatch = null)
        {
            Success = success;
            PatternMatch = patternMatch;
        }

        public bool Success { get; }
        public Match PatternMatch { get; }
    }
}