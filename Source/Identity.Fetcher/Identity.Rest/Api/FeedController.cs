using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Antlr.Runtime;
using Identity.Infrastructure.DTO;

namespace Identity.Rest.Api
{
    public class FeedController : ApiController
    {
        public IEnumerable<Post> Get()
        {
            return new[]
            {
                new Post
                {
                    Created = DateTime.Now,
                    Description = "asadgg",
                    Id = 1,
                    Tags = new List<string>(),
                    Title = "hello",
                    Type = "link",
                    Uri =
                        "http://www.michaelfcollins3.me/blog/2013/07/18/introduction-to-the-tpl-dataflow-framework.html"
                }
            };
        }
    }
}
