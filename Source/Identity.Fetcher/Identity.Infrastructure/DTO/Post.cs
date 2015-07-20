using System;
using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class Post
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Uri { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public IList<string> Tags { get; set; }

        public IList<Comment> Comments { get; set; }

        public DateTime Created { get; set; }

        public User Author { get; set; }

        public int CommentCount { get; set; }

        public int Upvotes { get; set; }

        public bool Read { get; set; }

        public bool Starred { get; set; }

        public bool Liked { get; set; }

        public bool Saved { get; set; }

        public DateTime Added { get; set; }
    }
}