﻿using System;
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
    [UnitOfWorkCommit]
    public class RssFeederController : ApiController
    {
        private readonly ChannelRepository channelRepo;
        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;

        private Domain.User user;

        public RssFeederController(ChannelRepository channelRepo, UserRepository userRepo, ILoadDtos dtoLoader)
        {
            this.channelRepo = channelRepo;
            this.userRepo = userRepo;
            this.dtoLoader = dtoLoader;
            user = userRepo.FindByName("jimmy");
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
