namespace LogDigger.Business.Models
{
    public interface IParserSelector
    {
        ILogParser GetParser(LogFile file);
    }
}