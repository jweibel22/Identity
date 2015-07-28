﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Post = Identity.Infrastructure.DTO.Post;

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class PostController : ApiController
    {
        private readonly CommentRepostitory commentRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;
        private Domain.User user;

        public PostController(ILoadDtos dtoLoader, PostRepository postRepo, UserRepository userRepo, CommentRepostitory commentRepo)
        {
            this.dtoLoader = dtoLoader;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.commentRepo = commentRepo;
            user = userRepo.FindByName("jimmy");
        }

        public IEnumerable<Post> Get(string tag)
        {
            return dtoLoader.LoadPosts(postRepo.FindByTag(tag));
        }

        [HttpPut]
        public void Upvote(long id)
        {

        }

        [HttpGet]
        public Post Get(long id)
        {
            return dtoLoader.LoadPost(user, postRepo.GetById(id, user.Id));
        }

        [HttpPost]
        public Infrastructure.DTO.Comment Comments(long id, Infrastructure.DTO.Comment comment)
        {
            var newComment = new Comment
            {
                Created = DateTime.Now,
                PostId = id,
                Text = comment.Body,
                UserId = user.Id,
                Author = user.Username
            };

            commentRepo.AddComment(newComment);
            
            return Mapper.Map<Infrastructure.DTO.Comment>(newComment);
        }

        [HttpPost]
        public void Post(long id, Post post)
        {
            var x  = postRepo.GetById(id, user.Id);
            x.Title = post.Title;
            x.Description = post.Description;

            postRepo.UpdatePost(x);
            postRepo.TagPost(x.Id, post.Tags);
        }

        [HttpPost]
        public void Read(long id, long userId)
        {
            userRepo.Read(userId, id);
        }
    }
}
