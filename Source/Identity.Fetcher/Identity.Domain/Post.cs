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

        //TODO: do we need this?
        //public int AuthorId { get; set; } 
    }
}