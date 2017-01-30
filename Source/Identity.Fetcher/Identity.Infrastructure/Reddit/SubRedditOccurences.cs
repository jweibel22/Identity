using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Reddit;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Directory = Lucene.Net.Store.Directory;

namespace Identity.Infrastructure.Services
{
    public class SubRedditOccurences : IDisposable
    {
        private readonly RedditIndexRepository repo;
        private readonly RedditIndex index;
        private readonly Lucene.Net.Store.Directory directory;

        private const int QueryLimit = 100000;
        private const int MinOccurences = 5;

        public SubRedditOccurences(RedditIndexRepository repo, RedditIndex index, LuceneDirectoryFactory luceneDirectoryFactory)
        {
            this.repo = repo;
            this.index = index;
            directory = luceneDirectoryFactory.Create(index.Id, index.StorageLocation);
        }

        public Text Add(string s, string type)
        {
            var allSubReddits = index.AllSubReddits;
            var text = repo.GetText(index.Id, s);
            if (text == null)
            {
                text = new Text {Content = s, Type = type};
                repo.AddText(index.Id, text);

                var occurences = SearchLuceneIndex(s).Where(kv => allSubReddits.ContainsKey(kv.Key) && kv.Value >= MinOccurences).Select(kv => new Occurences
                {
                    Count = kv.Value,
                    IndexId = index.Id,
                    SubRedditId = allSubReddits[kv.Key],
                    TextId = text.Id
                });

                repo.AddOccurences(occurences);
            }
            return text;
        }

        public IEnumerable<Text> FindSubstrings(string text)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Occurences> FindOccurences(IEnumerable<long> textIds)
        {
            return repo.FindOccurences(index.Id, textIds);
        }

        private IDictionary<string, int> SearchLuceneIndex(string text)
        {
            using (var searcher = new IndexSearcher(directory))
            {
                var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "text", analyzer);
                var query = parser.Parse(text);
                var hits = searcher.Search(query, QueryLimit);
                var subreddits = new Dictionary<string, int>();

                for (int i = 0; i < hits.ScoreDocs.Length; i++)
                {
                    var doc = searcher.Doc(hits.ScoreDocs[i].Doc);
                    var subreddit = doc.Get("subreddit");
                    if (!subreddits.ContainsKey(subreddit))
                    {
                        subreddits.Add(subreddit, 0);
                    }
                    subreddits[subreddit] = subreddits[subreddit] + 1;
                }

                return subreddits;
            }
        }

        public void Dispose()
        {
            directory.Dispose();
        }
    }
}
