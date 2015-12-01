using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Identity.Infrastructure
{
    class PerfCounter : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly string label;

        public PerfCounter(string label)
        {
            this.label = label;
            stopwatch.Start();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            log.Debug(String.Format("{0} took {1} ms", label, stopwatch.ElapsedMilliseconds));
        }
    }
}
