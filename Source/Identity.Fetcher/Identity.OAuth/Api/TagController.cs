using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Identity.Rest.Api
{
    [Authorize]
    [UnitOfWorkCommit]
    public class TagController : ApiController
    {
    }
}
