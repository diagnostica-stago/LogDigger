using System.Collections.Generic;
using LogDigger.Business.Models;

namespace LogDigger.Business.Services
{
    public interface ILogTemplateService
    {
        IReadOnlyList<CustomLogTemplate> GetTemplates();
        bool SaveTemplate(IReadOnlyList<CustomLogTemplate> templates);
    }
}