using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;

namespace Identity.Rest.Api
{
    public class RssFeederController : ApiController
    {
        private readonly IDbConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
        private readonly ChannelRepository channelRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;
        private readonly CommentRepostitory commentRepo;

        private Domain.User user;

        public RssFeederController()
        {
            userRepo = new UserRepository(con);
            channelRepo = new ChannelRepository(con);
            postRepo = new PostRepository(con);
            commentRepo = new CommentRepostitory(con);
            user = userRepo.FindByName("jimmy");
            dtoLoader = new DtoLoader(postRepo, commentRepo, user, userRepo, channelRepo);
        }

        [HttpGet]
        public RssFeeder Get(long id)
        {
            return dtoLoader.LoadRssFeeder(channelRepo.RssFeederById(id));
        }

        [HttpPut]
        public void Update(long id, RssFeeder rssFeeder)
        {
            channelRepo.UpdateTagsOfRssFeeder(id, rssFeeder.Tags);
        }
    }
}
