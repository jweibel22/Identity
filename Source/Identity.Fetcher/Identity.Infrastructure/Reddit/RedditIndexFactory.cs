using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Reddit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Identity.Infrastructure.Services
{
    public class RedditIndexFactory
    {
        private const int QueryLimit = 100000;

        private readonly LuceneDirectoryFactory luceneDirectoryFactory;
        private readonly RedditIndexRepository redditIndexRepository;
        

        public RedditIndexFactory(RedditIndexRepository redditIndexRepository, LuceneDirectoryFactory luceneDirectoryFactory)
        {
            this.redditIndexRepository = redditIndexRepository;
            this.luceneDirectoryFactory = luceneDirectoryFactory;
        }

        private static bool IsASCII(string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        private static bool ShouldWeAddSubmissionToIndex(Submission s)
        {
            return !String.IsNullOrEmpty(s.title) && !String.IsNullOrEmpty(s.subreddit) && s.subreddit.Length > 2 &&
                   s.subreddit.Length < 100 && IsASCII(s.subreddit);
        }

        private RedditIndex CreateIndex(IndexStorageLocation storageLocation, IEnumerable<Submission> submissions)
        {
            var subreddits = new Dictionary<string, int>();
            foreach (var submission in submissions.Where(ShouldWeAddSubmissionToIndex))
            {
                if (!subreddits.ContainsKey(submission.subreddit))
                {
                    subreddits.Add(submission.subreddit, 0);
                }

                subreddits[submission.subreddit] = subreddits[submission.subreddit] + 1;
            }

            return new RedditIndex
            {
                StorageLocation = storageLocation,
                SubReddits = subreddits.Select(kv => new SubReddit
                {
                    Name = kv.Key,
                    PostCount = kv.Value
                }).ToList()
            };
        }

        public void RebuildLuceneIndex(RedditIndex index, IEnumerable<Submission> submissions)
        {
            var allSubReddits = index.AllSubReddits;
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            var directory = luceneDirectoryFactory.Create(index.Id, index.StorageLocation);

            using (var writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                int id = 0;

                foreach (var submission in submissions.Where(s => ShouldWeAddSubmissionToIndex(s) && allSubReddits.ContainsKey(s.subreddit)))
                {
                    id++;
                    var doc = new Document();
                    doc.Add(new Field("id", id.ToString(), Field.Store.YES, Field.Index.NO));
                    doc.Add(new Field("text", submission.title, Field.Store.YES, Field.Index.ANALYZED));
                    doc.Add(new Field("subreddit", submission.subreddit, Field.Store.YES, Field.Index.NO));
                    writer.AddDocument(doc);
                }

                writer.Optimize();
                writer.Commit();
                writer.Flush(true, true, true);
            }
        }

        public void Build(IndexStorageLocation storageLocation, IEnumerable<Submission> submissions)
        {                        
            var index = CreateIndex(storageLocation, submissions);
            redditIndexRepository.AddRedditIndex(index);
            RebuildLuceneIndex(index, submissions);
        }
    }
}