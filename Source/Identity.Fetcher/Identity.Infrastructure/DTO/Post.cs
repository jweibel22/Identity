﻿using System;
using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class SimplePost
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Uri { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }        

        public DateTimeOffset Created { get; set; }

        public int PosterId { get; set; }

        public string PosterUsername { get; set; }

        public bool Read { get; set; }

        public bool Starred { get; set; }

        public bool Liked { get; set; }

        public bool Saved { get; set; }

        public DateTimeOffset Added { get; set; }

        public string Teaser { get; set; }

        public string EmbeddedUrl { get; set; }
    }

    public class Post
    {
        public Post()
        {
            Tags = new List<string>();
            Comments = new List<Comment>();
            PublishedIn = new List<Channel>();
        }

        public long Id { get; set; }

        public string Title { get; set; }

        public string Uri { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public IList<string> Tags { get; set; }

        public IList<Comment> Comments { get; set; }

        public DateTimeOffset Created { get; set; }

        public int PosterId { get; set; }

        public string PosterUsername { get; set; }

        public int CommentCount { get; set; }

        public int Upvotes { get; set; }

        public bool Read { get; set; }

        public bool Starred { get; set; }

        public bool Liked { get; set; }

        public bool Saved { get; set; }

        public DateTimeOffset Added { get; set; }

        public bool IsCollapsed { get; set; }

        public string Teaser { get; set; }

        public bool Expandable { get; set; }

        public int Popularity { get; set; }

        public int UserSpecificPopularity { get; set; }

        public IList<Channel> PublishedIn { get; set; }

        public string EmbeddedUrl { get; set; }
    }
}