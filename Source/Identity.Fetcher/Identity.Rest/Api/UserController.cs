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

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class UserController : ApiController
    {
        private readonly UserRepository userRepo;

        public UserController(UserRepository userRepo)
        {
            this.userRepo = userRepo;
        }

        public User Get()
        {
            var user = userRepo.FindByName("jimmy");

            var result = new User
            {
                Id = user.Id,
                DisplayName = user.Username,
                Feed = new List<Post>(),
                FollowsChannels = userRepo.Follows(user.Id).Select(c => Mapper.Map<Channel>(c)).ToList(),
                FollowsTags = new List<string>(),
                Owns = userRepo.Owns(user.Id).Select(c => Mapper.Map<Channel>(c)).ToList(),
                SavedChannel = user.SavedChannel,
                StarredChannel = user.StarredChannel,
                LikedChannel = user.LikedChannel,
                TagCloud = userRepo.GetTagCloud(user.Id).Select(Mapper.Map<Infrastructure.DTO.WeightedTag>).ToList()
            };

            return result;
        }
    }
}
