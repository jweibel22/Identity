using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Domain.RedditIndexes
{
    public class RedditIndex
    {
        private readonly Lazy<IDictionary<string, long>> allSubReddits;

        public IDictionary<string, long> AllSubReddits
        {
            get { return allSubReddits.Value; }
        }

        public RedditIndex()
        { 
           allSubReddits = new Lazy<IDictionary<string, long>>(() => SubReddits.ToDictionary(sr => sr.Name, sr => sr.Id));
        }

        public long Id { get; set; }

        public IndexStorageLocation StorageLocation { get; set; }

        public IList<SubReddit> SubReddits { get; set; }

        public long TotalPostCount { get; set; }
    }

    public class FullRedditIndex
    {
        public long Id { get; set; }

        public IList<SubReddit> SubReddits { get; private set; }

        public IEnumerable<Identity.Domain.RedditIndexes.Text> Texts { get; private set; }

        public IEnumerable<Occurences> Occurences { get; private set; }

        public long TotalPostCount { get; private set; }

        public FullRedditIndex(long id, IList<SubReddit> subReddits, IEnumerable<Text> texts, IEnumerable<Occurences> occurences, long totalPostCount)
        {
            Id = id;
            SubReddits = subReddits;
            Texts = texts;
            Occurences = occurences;
            TotalPostCount = totalPostCount;
        }
    }
}