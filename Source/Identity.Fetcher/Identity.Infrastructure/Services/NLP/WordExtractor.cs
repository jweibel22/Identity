using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveonik.Stemmers;

namespace Identity.Infrastructure.Services.NLP
{
    public class WordExtractionOptions
    {
        public bool Stem { get; set; }

        public bool Lemmatize { get; set; }

        public bool RemovePunctuation { get; set; }

        public bool IgnoreCommonWords { get; set; }
    }

    public interface IWordExtractor
    {
        IEnumerable<string> GetWords(string title);
    }

    public class WordExtractor : IWordExtractor
    {
        private IDictionary<string, bool> CommonWords { get; }
        private IDictionary<string, bool> CommonNouns { get; }

        private string[] ignoredCharacters = new[] { ":", ",", ";", "»", "«", "'", "?", "!", "(", ")", "[", "]", "{", "}", "\"", "'", "#", "*", "~", "`", "…", "|", "“", "”", "-", "–", "_", "." };

        private readonly EnglishStemmer stemmer = new EnglishStemmer();
        private readonly WordExtractionOptions options;

        public WordExtractor(WordExtractionOptions options) : this(@"Resources\commonwords-english.txt", @"Resources\nouns.csv", options)
        {
            
        }

        public WordExtractor(string commonWordsFile, string commonNounsFile, WordExtractionOptions options)
        {
            this.options = options;
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

        private string RemovePunctuation(string s)
        {
            //TODO: ignore all emojis. like: \ud83d\ude18            
            var result = s;
            foreach (var x in ignoredCharacters)
            {
                result = result.Replace(x, " ");
            }
            return result;
        }


        public IEnumerable<string> GetWords(string title)
        {
            title = title.Trim().ToLower();

            if (options.RemovePunctuation)
            {
                title = RemovePunctuation(title);
            }

            title = title.Replace("-", " ").Replace("–", " ").Replace("_", " ").Replace(".", " ");

            var tokens = title.Split(' ').AsEnumerable().Where(token => !String.IsNullOrEmpty(token) && token.Length > 2 && token.Length <= 100 && IsASCII(token));

            if (options.Stem)
            {
                tokens = tokens.Select(token => stemmer.Stem(token));
            }

            if (options.IgnoreCommonWords)
            {
                tokens = tokens.Where(token => !CommonWords.ContainsKey(token));
            }

            return tokens;
        }
    }
}
