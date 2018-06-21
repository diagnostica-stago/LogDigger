using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogDigger.Business.Models;
using LogDigger.Business.Utils;

namespace LogDigger.Business.Services
{
    public class LogExtractionService : ILogExtractionService
    {
        private readonly IParserSelector _parserSelector;
        private readonly IModuleClassifier _moduleClassifier;
        private IList<string> _folderToDeleteOnExit;

        public LogExtractionService(IClosingAppHandler closingAppHandler, IParserSelector parserSelector, IModuleClassifier moduleClassifier)
        {
            _parserSelector = parserSelector;
            _moduleClassifier = moduleClassifier;
            _folderToDeleteOnExit = new List<string>();
            closingAppHandler.ClosingApp += OnClosingApp;
        }

        private void OnClosingApp(object sender, EventArgs e)
        {
            foreach (var tempFolder in _folderToDeleteOnExit)
            {
                // if temp path, clear all file after load
                if (!string.IsNullOrEmpty(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        public FetchResult<List<LogFile>> FetchLogFiles(string path)
        {
            // _parserSelector.SetTemplate(@"{date} <#> {thread} <#> {level:Level} <#> {logger} <#> {content} <#> {id} <#> {exception} <#>" + Environment.NewLine);
            var isTempPath = false;
            var tempFolder = string.Empty;
            string extractionPath;
            var extensionFilter = @".*\.log(\.[0-9]+)?";
            if (File.Exists(path))
            {
                // zip file
                var extension = Path.GetExtension(path);
                if (extension == ".zip")
                {
                    var outFolder = PathUtil.GetTemporaryDirectory();
                    ZipUtil.ExtractZipFile(path, string.Empty, extensionFilter, outFolder);
                    tempFolder = outFolder;
                    extractionPath = outFolder;
                }
                else
                {
                    return new FetchResult<List<LogFile>>($"Format invalide : '{extension}'.");
                }
            }
            else
            {
                extractionPath = path;
            }
            if (Directory.Exists(extractionPath))
            {
                var files = new List<LogFile>();
                foreach (var file in EnumerateFiles(extractionPath, extensionFilter))
                {
                    var logFile = new LogFile(file, _parserSelector, _moduleClassifier);
                    files.Add(logFile);
                }
                _folderToDeleteOnExit.Add(tempFolder);
                return files.ToFetchResult(tempFolder);
            }
            return new FetchResult<List<LogFile>>("Le chemin n'existe pas.");
        }

        public static IEnumerable<string> EnumerateFiles(string enumPath, string pattern)
        {
            Regex reg = new Regex(pattern);

            return Directory.GetFiles(enumPath, "*.*", SearchOption.AllDirectories)
                                 .Where(path => reg.IsMatch(path))
                                 .ToList();
        }
    }
}