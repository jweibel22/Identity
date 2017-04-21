using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using AutoMapper;
using CsQuery.ExtensionMethods;
using HtmlAgilityPack;
using Identity.Domain;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.OAuth;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Comment = Identity.Domain.Comment;
using Post = Identity.Infrastructure.DTO.Post;
using ReadHistory = Identity.OAuth.Models.ReadHistory;

namespace Identity.Rest.Api
{
    [Authorize]
    [UnitOfWorkCommit]
    public class PostController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CommentRepostitory commentRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;
        private Domain.User user;
        private IArticleContentsFetcher articleContentsFetcher;

        public PostController(ILoadDtos dtoLoader, PostRepository postRepo, UserRepository userRepo, CommentRepostitory commentRepo, IArticleContentsFetcher articleContentsFetcher)
        {
            this.dtoLoader = dtoLoader;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.commentRepo = commentRepo;
            this.articleContentsFetcher = articleContentsFetcher;

            var identity = User.Identity as ClaimsIdentity;
            user = userRepo.FindByName(identity.Name);
        }

        public IEnumerable<Post> Get(string tag)
        {
            var results = postRepo.FindByTitleOrTag(tag).Select(Mapper.Map<Post>).ToList(); 
            results.ForEach(p => p.IsCollapsed = true);
            return results;
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
        [Route("Api/Post/{id}/Comments")]
        public Infrastructure.DTO.Comment Comments(long id, Infrastructure.DTO.Comment comment)
        {
            var newComment = new Comment
            {
                Created = DateTimeOffset.Now,
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

            if (post.Tags != null)
            {
                postRepo.TagPost(x.Id, post.Tags);    
            }            
        }

        [HttpPost]
        public void Read(long id, long userId)
        {
            userRepo.Read(userId, id);
        }

        [HttpPost]
        [Route("Api/Post/Read")]
        public void Read(long userId, ReadHistory readHistory)
        {
            foreach (var id in readHistory.PostIds)
            {
                userRepo.Read(userId, id);    
            }                 
        }

        [HttpPost]
        [Route("Api/Post/ReadAndDecrementUnreadCount")]
        public void ReadAndDecrementUnreadCount(long userId, long channelId, ReadHistory readHistory)
        {
            var insertCount = 0;
            foreach (var id in readHistory.PostIds)
            {
                insertCount += userRepo.Read(userId, id);
            }

            if (insertCount > 0)
            {
                userRepo.DecrementUnreadCount(userId, channelId, insertCount);
            }
            
        }

        [HttpGet]
        [Route("Api/Post/History")]
        public IEnumerable<Post> History()
        {
            var posts = postRepo.ReadHistory(user.Id).Select(Mapper.Map<Post>).ToList();
                            
            foreach (var post in posts)
            {
                post.IsCollapsed = true;
            }

            return posts;
        }

        [HttpGet]
        [Route("Api/Post/{id}/Contents")]
        public HttpResponseMessage FetchContents(long id)
        {
            var post = postRepo.GetById(id, user.Id);

            var url = post.Uri;

            var contents = articleContentsFetcher.Fetch(url);

            return new HttpResponseMessage()
            {
                Content = new StringContent(contents, Encoding.UTF8, "text/html")
            };
        }
    }
}
