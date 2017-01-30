using System;

namespace Identity.Domain.RedditIndexes
{
    public class SuggestedSubRedditsDebugInfo
    {
        public long Id { get; set; }
        public string EntityName { get; set; }
        public string Name { get; set; }
        public int PostCount { get; set; }
        public int Occurences { get; set; }

        private static double Log(double d)
        {
            return Math.Round(Math.Log(d), 2);
        }

        public double LogOccurences
        {
            get
            {
                return Log(Occurences);
            }
        }
        public double LogPostCount
        {
            get { return Log(PostCount); }
        }

        public int TotalEntityOccurences { get; set; }
        public double LogTotalEntityOccurences { get { return Log(TotalEntityOccurences); } }
        public long TotalRedditPosts { get; set; }
        public double LogTotalRedditPosts { get { return Log(TotalRedditPosts); } }

        public double SubRedditFreq { get { return Math.Round((double)Occurences / (double)PostCount, 2); } }
        public double TotalFreq { get { return Math.Round((double)TotalEntityOccurences / (double)TotalRedditPosts, 10); } }

        public double LogSubRedditFreq { get { return Math.Round(LogOccurences / LogPostCount, 2); } }
        public double LogTotalFreq { get { return Math.Round(LogTotalEntityOccurences / LogTotalRedditPosts, 2); } }
        public double Score { get { return Math.Round(LogSubRedditFreq / LogTotalFreq, 2); } }
        public double ScoreX { get { return Math.Round(SubRedditFreq / LogTotalFreq, 2); } }
    }
}