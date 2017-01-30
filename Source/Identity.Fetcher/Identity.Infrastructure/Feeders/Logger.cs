using System;
using System.IO;
using System.Reflection;
using log4net;

namespace Identity.Infrastructure.Rss
{
    class Logger
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TextWriter azureLog;

        public Logger(TextWriter azureLog)
        {
            this.azureLog = azureLog;
        }

        public void Info(string message)
        {
            log.Info(message);
            azureLog.WriteLine(message);
        }

        public void Error(string message, Exception ex)
        {
            log.Error(message, ex);
            azureLog.WriteLine(String.Format("{0}. Reason: {1}", message, ex));
        }
    }
}