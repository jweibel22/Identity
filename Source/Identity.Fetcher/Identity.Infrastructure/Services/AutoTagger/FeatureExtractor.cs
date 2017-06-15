using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Services.NLP;

namespace Identity.Infrastructure.Services.AutoTagger
{
    public interface IFeatureExtractor
    {
        IDictionary<long, double> ComputeFeatureVectors(IList<long> textIds);
        IEnumerable<IDictionary<long, double>> ComputeFeatureVectors(IList<PostAndId> posts);
    }

    public class TextExtractor
    {
        private readonly IWordExtractor wordExtractor;
        private readonly IEnumerable<Tuple<long, string>> nlpTexts;
        private readonly IDictionary<string, long> allTexts;

        public TextExtractor(IWordExtractor wordExtractor, IEnumerable<Tuple<long, string>> nlpTexts, FullRedditIndex index)
        {
            this.wordExtractor = wordExtractor;
            this.nlpTexts = nlpTexts;
            this.allTexts = index.Texts.ToDictionary(x => x.Content, x=> x.Id);
        }

        public IList<PostAndText> GetTexts(IList<PostAndId> posts)
        {
            var textsInPosts = posts
                                    .SelectMany(p => wordExtractor.GetWords(p.Title).Where(w => allTexts.ContainsKey(w)).Select(w => new PostAndText { PostId = p.PostId, TextId = allTexts[w] }))
                                    .ToList();

            if (nlpTexts != null)
            {
                var xx = posts
                    .SelectMany(p =>
                            nlpTexts.Where(t => t.Item1 == p.PostId && allTexts.ContainsKey(t.Item2))
                                .Select(w => new PostAndText { PostId = p.PostId, TextId = allTexts[w.Item2] }));

                textsInPosts = textsInPosts.Union(xx).Distinct().ToList();
            }

            return textsInPosts;
        }
    }

    public class FeatureExtractor : IFeatureExtractor
    {
        private readonly IEnumerable<IGrouping<long, Occurences>> occurences;
        private readonly FullRedditIndex index;
        private readonly IDictionary<long, int> subredditPostCounts;
        private readonly IDictionary<long, string> subredditNames;
        private readonly IDictionary<long, int> totalTextOccurences;
        private readonly IDictionary<long, string> textIdToContent;
        private readonly TextExtractor textExtractor;

        public FeatureExtractor(FullRedditIndex index, Func<Occurences, bool> filter, TextExtractor textExtractor)
        {
            this.index = index;
            this.textExtractor = textExtractor;
            this.occurences = index.Occurences.Where(filter).GroupBy(o => o.TextId).ToList();
            this.textIdToContent = index.Texts.ToDictionary(t => t.Id, t => t.Content);
            this.subredditPostCounts = index.SubReddits.ToDictionary(sr => sr.Id, sr => sr.PostCount);
            this.subredditNames = index.SubReddits.ToDictionary(sr => sr.Id, sr => sr.Name);
            this.totalTextOccurences = occurences.ToDictionary(x => x.Key, x => x.Sum(ff => ff.Count));
        }

        private IDictionary<long, double> newSuggest4(IEnumerable<IGrouping<long, Occurences>> xx, int textCount)
        {
            var logTotalRedditPosts = Math.Log(index.TotalPostCount);

            Func<IEnumerable<Occurences>, double> scorer = g =>
                g.Sum(a => (Math.Log(a.Count) / Math.Log(subredditPostCounts[a.SubRedditId])) / (Math.Log(totalTextOccurences[a.TextId]) / logTotalRedditPosts)) / (textCount);

            //	Func<IEnumerable<Occurences>, double> scorer = g =>
            //		g.Max(a => (Math.Log(a.Count) / Math.Log(subredditPostCounts[a.SubRedditId])) / (Math.Log(totalCounts[a.TextId]) / logTotalRedditPosts));

            var weighted = xx.SelectMany(o => o)
                            .GroupBy(x => x.SubRedditId)
                            .Select(g => new SubRedditScore { Id = g.Key, Score = scorer(g) });

            var result = weighted.ToDictionary(w => w.Id, w => w.Score);

            return result;
        }

        public IDictionary<long, double> ComputeFeatureVectors(IList<long> textIds)
        {
            var occ = occurences.Where(o => textIds.Contains(o.Key));
            var result = newSuggest4(occ, textIds.Count);
            return result;
        }
        
        public IList<SuggestedSubRedditsDebugInfo> DebugInfo(IList<long> textIds)
        {
            var occ = occurences.Where(o => textIds.Contains(o.Key));
            var totalRedditPosts = index.TotalPostCount;

            var temp = occ.SelectMany(o => o).Select(x => new SuggestedSubRedditsDebugInfo
            {
                Id = x.SubRedditId,
                EntityName = textIdToContent[x.TextId],
                Name = subredditNames[x.SubRedditId],
                Occurences = x.Count,
                PostCount = subredditPostCounts[x.SubRedditId],
                TotalEntityOccurences = textIds.Count,
                TotalRedditPosts = totalRedditPosts
            }).OrderBy(x => x.Name).ThenByDescending(x => x.Score).ToList();
            //.Where(x => x.LogSubRedditFreq > 0.2)

            return temp;
        }

        public IEnumerable<IList<SuggestedSubRedditsDebugInfo>> DebugInfo(IList<PostAndId> posts)
        {
            var textsInPosts = textExtractor.GetTexts(posts);

            foreach (var p in posts)
            {
                var textIds = textsInPosts.Where(tip => tip.PostId == p.PostId).Select(t => t.TextId).ToList();
                yield return DebugInfo(textIds);
            }
        }

        public IEnumerable<IDictionary<long, double>> ComputeFeatureVectors(IList<PostAndId> posts)
        {
            var textsInPosts = textExtractor.GetTexts(posts);

            foreach (var p in posts)
            {
                var textIds = textsInPosts.Where(tip => tip.PostId == p.PostId).Select(t => t.TextId).ToList();
                yield return ComputeFeatureVectors(textIds);
            }
        }
    }
}
 