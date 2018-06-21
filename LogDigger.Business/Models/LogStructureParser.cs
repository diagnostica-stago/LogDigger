using System.Linq;
using Sprache;

namespace LogDigger.Business.Models
{
    public class LogStructureParser
    {
        private static readonly Parser<char> Brackets = Parse.Char('{').Or(Parse.Char('}'));

        public static readonly Parser<string> Separator =
            Parse.Char('\\').Then(x => Brackets)
                .Or(Parse.AnyChar.Except(Brackets))
                .Many()
                .Select(chars => new string(chars.ToArray()));

        public static readonly Parser<FieldContent> FieldContent =
            from lbracket in Parse.Char('{')
            from name in Parse.AnyChar.Except(Brackets.Or(Parse.Char(':'))).Many()
            from sep in Parse.Char(':').Optional()
            from type in Parse.AnyChar.Except(Brackets).Many().Optional()
            from rbracket in Parse.Char('}')
            select new FieldContent(new string(name.ToArray()), type.IsDefined ? new string(type.Get().ToArray()) : string.Empty);

        public static readonly Parser<FieldFormat> FieldFormat =
            from fieldContent in FieldContent.Optional()
            from separator in Separator
            select new FieldFormat(fieldContent.IsDefined ? fieldContent.Get().Name : string.Empty, fieldContent.IsDefined ? fieldContent.Get().Type : string.Empty, separator);

        public static readonly Parser<LogFormat> LogFormat = FieldFormat.Many().Select(x => new LogFormat(x.ToList()));
    }

    public class FieldContent
    {
        public FieldContent(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; }
    }
}