using System;

namespace Identity.Domain
{
    public class Post
    {
        public long Id { get; set; }

        public DateTime Created { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Uri { get; set; }

        public DateTime Added { get; set; }

        public bool Read { get; set; }

        public bool Starred { get; set; }

        public bool Liked { get; set; }

        public bool Saved { get; set; }

        public int Popularity { get; set; }

        //TODO: do we need this?
        //public int AuthorId { get; set; } 
    }
}