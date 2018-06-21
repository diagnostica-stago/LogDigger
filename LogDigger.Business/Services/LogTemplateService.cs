using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using LogDigger.Business.Models;
using LogDigger.Business.Utils;
using Newtonsoft.Json;

namespace LogDigger.Business.Services
{
    public class LogTemplateService : ILogTemplateService
    {
        public event EventHandler<EventArgs> TemplatesUpdated; 

        public IReadOnlyList<CustomLogTemplate> GetTemplates()
        {
            var templatesFile = Path.Combine(PathUtil.GetAppDataDirectory(), "templates.json");
            if (!File.Exists(templatesFile))
            {
                var assembly = Assembly.GetEntryAssembly();
                var manifestResourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.DefaultTemplates.json");
                var textStreamReader = new StreamReader(manifestResourceStream);
                File.WriteAllText(templatesFile, textStreamReader.ReadToEnd(), Encoding.UTF8);
            }
            var templatesStr = File.ReadAllText(templatesFile);
            var templates = JsonConvert.DeserializeObject<List<CustomLogTemplate>>(templatesStr);
            return templates;
        }

        public IReadOnlyList<CustomLogTemplate> LoadTemplates(string path)
        {
            var templatesFile = path;
            if (!File.Exists(templatesFile))
            {
                return new List<CustomLogTemplate>();
            }
            try
            {
                var templatesStr = File.ReadAllText(templatesFile);
                var templates = JsonConvert.DeserializeObject<List<CustomLogTemplate>>(templatesStr);
                return templates;
            }
            catch (Exception)
            {
                return new List<CustomLogTemplate>();
            }
        }

        public bool SaveTemplate(IReadOnlyList<CustomLogTemplate> templates)
        {
            return SaveTemplate(templates, Path.Combine(PathUtil.GetAppDataDirectory(), "templates.json"), true);
        }

        public bool SaveTemplate(IReadOnlyList<CustomLogTemplate> templates, string path, bool triggerUpdate)
        {
            var templatesFile = path;
            var templatesStr = JsonConvert.SerializeObject(templates);
            File.WriteAllText(templatesFile, templatesStr);
            // UserSettings.Default.Templates = templatesStr;
            if (triggerUpdate)
            {
                RaiseTemplatesUpdated();
            }
            return true;
        }

        protected virtual void RaiseTemplatesUpdated()
        {
            TemplatesUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}