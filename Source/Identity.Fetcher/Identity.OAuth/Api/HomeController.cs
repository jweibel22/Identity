using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.Rest;
using log4net;

namespace Identity.OAuth.Api
{
    [Authorize]
    [UnitOfWorkCommit]
    public class HomeController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChannelRepository channelRepo;
        private readonly PostRepository postRepo;

        private readonly ILoadDtos dtoLoader;
        private Domain.User user;

        public HomeController(ILoadDtos dtoLoader, ChannelRepository channelRepo, UserRepository userRepo, PostRepository postRepo)
        {
            var identity = User.Identity as ClaimsIdentity;
            user = userRepo.FindByName(identity.Name);
            this.dtoLoader = dtoLoader;
            this.channelRepo = channelRepo;
            this.postRepo = postRepo;
        }

        [HttpGet]
        public HomeScreen Get()
        {
            return new HomeScreen
            {
                Channels = dtoLoader.LoadChannelList(user, channelRepo.TopChannels(10, user.Id)).ToList(),
                Posts = dtoLoader.LoadPosts(user, postRepo.TopPosts(10, user.Id)).ToList(),
                TagCloud = postRepo.TopTags(20).Select(Mapper.Map<Infrastructure.DTO.WeightedTag>).ToList()
            };
        }
    }
}
