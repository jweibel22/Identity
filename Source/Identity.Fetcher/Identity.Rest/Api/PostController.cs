using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using CsQuery;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Post = Identity.Infrastructure.DTO.Post;

namespace Identity.Rest.Api
{
    public class PostController : ApiController
    {
        private readonly IDbConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
        private readonly ChannelRepository channelRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly ILoadDtos dtoLoader;
        private Domain.User user;

        public PostController()
        {
            postRepo = new PostRepository(con);
            channelRepo = new ChannelRepository(con);
            userRepo = new UserRepository(con);
            commentRepo = new CommentRepostitory(con);
            user = userRepo.FindByName("jimmy");
            dtoLoader = new DtoLoader(postRepo, commentRepo, user, userRepo, channelRepo);
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
            return dtoLoader.LoadPost(postRepo.GetById(id));
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
            var x  = postRepo.GetById(id);
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
