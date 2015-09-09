﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class UserController : ApiController
    {
        private readonly UserRepository userRepo;
        private readonly ChannelRepository channelRepo;

        public UserController(UserRepository userRepo, ChannelRepository channelRepo)
        {
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
        }

        public User Get(int id)
        {
            var user = userRepo.GetById(id);

            if (user == null)
            {
                return null;
            }

            return Map(user);
        }

        private User Map(Domain.User user)
        {
            return new User
            {
                Id = user.Id,
                DisplayName = user.Username,
                Feed = new List<Post>(),
                FollowsChannels = userRepo.Follows(user.Id).Select(c => Mapper.Map<Channel>(c)).ToList(),
                FollowsTags = new List<string>(),
                Owns = userRepo.Owns(user.Id).Select(c =>
                {
                    var channel = Mapper.Map<Channel>(c);
                    channel.UnreadCount = channelRepo.UnreadCount(user.Id, channel.Id);
                    return channel;
                }).ToList(),
                SavedChannel = user.SavedChannel,
                StarredChannel = user.StarredChannel,
                LikedChannel = user.LikedChannel,
                TagCloud = userRepo.GetTagCloud(user.Id).Select(Mapper.Map<Infrastructure.DTO.WeightedTag>).ToList()
            };
        }

        public User Get()
        {
            var identity = User.Identity as ClaimsIdentity;
            var user = userRepo.FindByName(identity.Name);

            if (user == null)
            {
                return null;
            }

            return Map(user);
        }

        [HttpGet]
        public IEnumerable<User> Get(string query)
        {
            return userRepo.SearchByName(query).Select(Map).ToList();
        }
    }
}