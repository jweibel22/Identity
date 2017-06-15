using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Helpers
{    
    public class EnglishLanguage
    {
        public IDictionary<string, bool> CommonWords { get; }
        public IDictionary<string, bool> CommonNouns { get; }

        private string[] ignoredCharacters = new[] { ":", ",", ";", "»", "«", "'", "?", "!", "(", ")", "[", "]", "{", "}", "\"", "'", "#", "*", "~", "`", "…", "|", "“", "”", "-", "–", "_", "." };

        public EnglishLanguage(): this(@"Resources\commonwords-english.txt", @"Resources\nouns.csv")
        {
            
        }

        public EnglishLanguage(string commonWordsFile, string commonNounsFile)
        {
            CommonWords =
                System.IO.File.ReadAllLines(commonWordsFile)
                    .Select(w => w.Trim().ToLower())
                    .Take(1000)
                    .Distinct()
                    .ToDictionary(w => w, w => true);

            CommonNouns = System.IO.File.ReadAllLines(commonNounsFile)
                            .Select(w => w.Trim().ToLower()).Distinct().ToDictionary(s => s, s => true);
        }

        private static bool IsASCII(string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public string IgnoreAll(string s)
        {
            var result = s;
            foreach (var x in ignoredCharacters)
            {
                result = result.Replace(x, " ");
            }
            return result;
        }


        public IEnumerable<string> GetWords(string title)
        {
            //TODO: ignore all emojis. like: \ud83d\ude18            
            var ss = IgnoreAll(title.Trim());
            var words = ss.Replace("-", " ").Replace("–", " ").Replace("_", " ").Replace(".", " ")
                    .ToLower().Split(' ').Where(word => !String.IsNullOrEmpty(word) && word.Length > 2 && word.Length <= 100 && !CommonWords.ContainsKey(word));

            return words;
        }

    }
}
