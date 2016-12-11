using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Domain.Clustering
{
    static class StringExtensions
    {
        public static string RemoveAll(this string s, string[] toRemove)
        {
            var result = s;

            foreach (var c in toRemove)
            {
                result = result.Replace(c, "");
            }

            return result;
        }
    }

    public class Algorithm
    {
        private const double threshold = 3;

        static bool IsInt(string s)
        {
            int i;
            return Int32.TryParse(s, out i);
        }

        static string[] Tokenize(string[] commonWords, string text)
        {
            var ignoredCharacters = new[] { ":", ",", "»", "«", "'" };
            return text.Trim()
                .Replace("-", " ").Replace("–", " ")
                .RemoveAll(ignoredCharacters)
                .ToLower()
                .Split(' ')
                .Where(word => !String.IsNullOrEmpty(word) && !commonWords.Contains(word) && !IsInt(word)).ToArray();
        }

        static IList<string> CreateVocabList(IEnumerable<string[]> dataSet)
        {
            return dataSet.SelectMany(l => l).Distinct().ToList();
        }

        static int[] GetWordVector(IEnumerable<string> vocabList, string[] text)
        {
            return vocabList.Select(word => text.Contains(word) ? 1 : 0).ToArray();
        }

        public static IList<Cluster> ComputeClusters(string[] commonWords, IList<Document> articles)
        {            
            var tokenized = articles.ToDictionary(a => a.Id, a => Tokenize(commonWords, a.Title + " " + a.Description.Trim()));
            var vocabList = CreateVocabList(tokenized.Values.ToList());

            foreach (var article in articles)
            {
                article.WordVector = GetWordVector(vocabList, tokenized[article.Id]);
            }

            var world = new World(threshold);

            foreach (var article in articles)
            {
                world.Add(article);
            }

            return world.Clusters.Where(c => c.Documents.Count > 1).ToList();
        }
    }
}