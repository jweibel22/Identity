using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
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
            return postRepo.FindByTitleOrTag(tag).Select(Mapper.Map<Post>); 
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

        [HttpGet]
        [Route("Api/Post/History")]
        public IEnumerable<Post> History()
        {
            return postRepo.ReadHistory(user.Id).Select(Mapper.Map<Post>);
        }
    }
}
