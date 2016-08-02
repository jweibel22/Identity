using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using AutoMapper;
using CsQuery.ExtensionMethods;
using HtmlAgilityPack;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.OAuth.Models;
using log4net;
using Comment = Identity.Domain.Comment;
using Post = Identity.Infrastructure.DTO.Post;

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

        public PostController(ILoadDtos dtoLoader, PostRepository postRepo, UserRepository userRepo, CommentRepostitory commentRepo)
        {
            this.dtoLoader = dtoLoader;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.commentRepo = commentRepo;

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

        [HttpGet]
        [Route("Api/Post/History")]
        public IEnumerable<Post> History()
        {
            return postRepo.ReadHistory(user.Id).Select(Mapper.Map<Post>);
        }

        private string ArticleCssSelector(string url)
        {
            if (url.Contains("http://jyllands-posten.dk") || url.Contains("http://finans.dk"))
                return "//div[@id=\"articleText\"]";
            else if (url.Contains("http://www.dr.dk") || url.Contains("http://dr.dk"))
                return "//div[@class=\"wcms-article-content\"]";      
            else if (url.Contains("https://ing.dk"))
                return "//section[@class=\"body\"]";
            else
                return null;            
        }

        [HttpGet]
        [Route("Api/Post/{id}/Contents")]
        public string FetchContents(long id)
        {
            var post = postRepo.GetById(id, user.Id);
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                var result = webClient.DownloadString(post.Uri);

                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                var selector = ArticleCssSelector(post.Uri);
                var elm = selector == null ? null : doc.DocumentNode.SelectSingleNode(selector);

                if (elm == null)
                {
                    return "";
                }
                else
                {
                    return elm.InnerHtml.Replace("\r\n", "").Replace("\n", "").Replace("\t", "").Replace("\"", "");
                }
            }
        }
    }
}
