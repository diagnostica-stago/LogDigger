using System;
using System.IO;

namespace LogDigger.Business.Utils
{
    public static class PathUtil
    {
        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), $"LogDigger.{Path.GetRandomFileName()}");
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static string GetAppDataDirectory()
        {
            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Stago\LogDigger");
            Directory.CreateDirectory(appDataDirectory);
            return appDataDirectory;
        }
    }
}