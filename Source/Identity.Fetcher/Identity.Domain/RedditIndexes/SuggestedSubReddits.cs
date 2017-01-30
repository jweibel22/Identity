using System.Collections.Generic;

namespace Identity.Domain.RedditIndexes
{
    public class SuggestedSubReddits
    {
        public long ArticleId { get; set; }

        public IList<SubRedditScore> TopSubReddits { get; set; }
    }
}