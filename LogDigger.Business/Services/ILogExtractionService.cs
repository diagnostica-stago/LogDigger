using System.Collections.Generic;
using LogDigger.Business.Models;

namespace LogDigger.Business.Services
{
    public interface ILogExtractionService
    {
        FetchResult<List<LogFile>> FetchLogFiles(string path);
    }
}