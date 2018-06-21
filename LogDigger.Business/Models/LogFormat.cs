using System.Collections.Generic;

namespace LogDigger.Business.Models
{
    public class LogFormat
    {
        public LogFormat(IReadOnlyList<FieldFormat> fields)
        {
            Fields = fields;
        }

        public IReadOnlyList<FieldFormat> Fields { get; }
    }
}