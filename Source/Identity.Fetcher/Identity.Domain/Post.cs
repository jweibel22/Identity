using System;

namespace Identity.Domain
{
    public class Post
    {
        public long Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Uri { get; set; }

        public DateTimeOffset Added { get; set; }

        public bool Read { get; set; }

        public bool Starred { get; set; }

        public bool Liked { get; set; }

        public bool Saved { get; set; }

        public int Popularity { get; set; }

        public int UserSpecificPopularity { get; set; }

        public int PosterId { get; set; }

        public string PosterUsername { get; set; }
    }
}