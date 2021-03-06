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

    public class PostGroup
    {
        public IList<Post> Posts { get; set; }
    }

    public class Post
    {
        private string _title;

        public Post()
        {
            Tags = new List<string>();
            Comments = new List<Comment>();
            PublishedIn = new List<ChannelReference>();
        }

        public long Id { get; set; }

        public string Title
        {
            get
            {
                var prefix = PremiumContent ? "[Premium] " : "";
                return prefix + _title;
            }
            set { _title = value; }
        }

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

        public IList<ChannelReference> PublishedIn { get; set; }

        public string EmbeddedUrl { get; set; }

        public bool CanBeInlined { get; set; }

        public bool PremiumContent { get; set; }

        public long? ClusterId { get; set; }
    }

    public class ChannelReference
    {
        public long Id { get; set; }

        public string Name { get; set; }        
    }
}