using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Identity.Infrastructure.Reddit
{
    public class Submission
    {
        public string title { get; set; }

        //public string url { get; set; }

        public string subreddit { get; set; }

        //public long created_utc { get; set; }

        //public string author { get; set; }

        public static IEnumerable<Submission> LoadFromJsonFile(string filename, Action<int> onProcessed = null)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    if (onProcessed != null)
                    {
                        onProcessed(++i);
                    }
                    var line = sr.ReadLine();
                    yield return JsonConvert.DeserializeObject<Submission>(line);
                }
            }

        }

        public static IEnumerable<Submission> LoadFromCsvFile(string file, Action<int> onProcessed = null)
        {
            using (var sr = new StreamReader(file))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    if (onProcessed != null)
                    {
                        onProcessed(++i);
                    }

                    var line = sr.ReadLine().Split(';');

                    if (line.Length < 2)
                        continue;

                    yield return new Submission
                    {
                        title = line[0],
                        subreddit = line[1]
                    };
                }
            }
        }
    }
}
