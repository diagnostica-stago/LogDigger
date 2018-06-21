using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogDigger.Business.Models
{
    public static class FileUtils
    {
        public static string[] WriteSafeReadAllLines(string path)
        {
            return Enumerable.ToArray<string>(WriteSafeReadLines(path));
        }

        public static IEnumerable<string> WriteSafeReadLines(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    while (!sr.EndOfStream)
                    {
                        yield return sr.ReadLine();
                    }
                }
            }
        }

        public static IEnumerable<int> WriteSafeReadAllChars(string path, int maxChar = Int32.MaxValue)
        {
            int current = 0;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    while (current < maxChar && !sr.EndOfStream)
                    {
                        current++;
                        yield return sr.Read();
                    }
                }
            }
        }

        public static List<int> WriteSafeReadAllCharsToList(string path)
        {
            var result = new List<int>();
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    while (!sr.EndOfStream)
                    {
                        result.Add(sr.Read());
                    }
                }
            }

            return result;
        }

        public static List<char> WriteSafeReadAllCharsBufferedToList(string path)
        {
            var result = new List<char>();
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    while (!sr.EndOfStream)
                    {
                        var bufferSize = 1024;
                        var buffer = new char[bufferSize];
                        sr.ReadBlock(buffer, 0, bufferSize);
                        result.AddRange(buffer);
                    }
                }
            }

            return result;
        }

        public static string WriteSafeReadAllText(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static TextReader WriteSafeReadAllCharsReader(string path)
        {
            return new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Default, true);
        }
    }
}