using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

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
        static bool IsInt(string s)
        {
            int i;
            return Int32.TryParse(s, out i);
        }

        static string[] Tokenize(string[] commonWords, string text)
        {
            var ignoredCharacters = new[] { ":", ",", "»", "«", "'" };
            return text.Trim()
                .Replace("-", " ").Replace("–", " ").Replace(".", " ")
                .RemoveAll(ignoredCharacters)
                .ToLower()
                .Split(' ')
                .Where(word => !String.IsNullOrEmpty(word) && !commonWords.Contains(word) && !IsInt(word)).ToArray();
        }

        static IList<string> CreateVocabList(IEnumerable<string[]> dataSet)
        {
            return dataSet.SelectMany(l => l).Distinct().ToList();
        }

        static IEnumerable<double> GetWordVector(IEnumerable<string> vocabList, string[] text)
        {
            return vocabList.Select(word => text.Contains(word) ? 1.0 : 0.0);
        }

        public static void CalculateWordVectors(string[] commonWords, IList<Document> articles)
        {
            var tokenized = articles.ToDictionary(a => a.Id, a => Tokenize(commonWords, a.Title + " " + a.Description.Trim()));
            var vocabList = CreateVocabList(tokenized.Values.ToList());

            foreach (var article in articles)
            {
                article.WordVector = Vector<double>.Build.SparseOfEnumerable(GetWordVector(vocabList, tokenized[article.Id]));
            }
        }
    }
}