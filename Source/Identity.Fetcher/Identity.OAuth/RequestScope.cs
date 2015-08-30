using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Identity.OAuth
{
    public static class RequestScope
    {
        static RequestScope()
        {
            Scope = new ThreadLocal<ScopeObject>();
        }

        public static ThreadLocal<ScopeObject> Scope { get; private set; }
    }

    public class ScopeObject
    {
    }
}