namespace LogDigger.Business.Models
{
    public class CustomParserSelector : IParserSelector
    {
        private readonly ILogParser _parser;

        public CustomParserSelector(ILogParser parser)
        {
            _parser = parser;
        }
        
        public ILogParser GetParser(LogFile file)
        {
            return _parser;
        }
    }
}