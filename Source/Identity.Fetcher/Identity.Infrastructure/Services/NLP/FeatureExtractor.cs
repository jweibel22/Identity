using System.Collections.Generic;
using System.Linq;

namespace Identity.Infrastructure.Services.NLP
{
    public class FeatureExtractor
    {
        private readonly WordExtractor wordExtractor;
        private readonly IList<string> vocabulary;

        public FeatureExtractor(WordExtractor wordExtractor, IEnumerable<string> allText)
        {
            this.wordExtractor = wordExtractor;
            vocabulary = allText.SelectMany(wordExtractor.GetWords).Distinct().ToList();
        }

        public double[] GetFeatureVector(string text)
        {
            var tokens = wordExtractor.GetWords(text).GroupBy(w => w).ToDictionary(t => t.Key, t => t.Count());
            return vocabulary.Select(word => tokens.ContainsKey(word) ? tokens[word] : 0.0).ToArray();
        }
    }
}