using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Antlr.Runtime;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using log4net;

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class FeedController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;

        private readonly Domain.User user;

        public FeedController(ILoadDtos dtoLoader, UserRepository userRepo)
        {
            this.dtoLoader = dtoLoader;
            this.userRepo = userRepo;
            user = userRepo.FindByName("jimmy");
        }

        [HttpGet]
        public IEnumerable<Post> Get(DateTime timestamp, int fromIndex, string orderBy)
        {
            log.Info("Paging: " + fromIndex);
            return dtoLoader.LoadPosts(userRepo.GetFeed(user.Id, timestamp, fromIndex, orderBy));
        }
    }
}
