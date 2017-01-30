namespace Identity.Domain.RedditIndexes
{
    public class Occurences
    {
        public long TextId { get; set; }

        public long SubRedditId { get; set; }

        public long IndexId { get; set; }

        public int Count { get; set; }
    }
}