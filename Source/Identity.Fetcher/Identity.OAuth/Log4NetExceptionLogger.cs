using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http.ExceptionHandling;
using log4net;

namespace Identity.Rest
{
    public class Log4NetExceptionLogger : ExceptionLogger
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public override void Log(ExceptionLoggerContext context)
        {
            log.Error(RequestToString(context.Request), context.Exception);
        }

        private static string RequestToString(HttpRequestMessage request)
        {
            var message = new StringBuilder();
            if (request.Method != null)
                message.Append(request.Method);

            if (request.RequestUri != null)
                message.Append(" ").Append(request.RequestUri);

            return message.ToString();
        }
    }
}