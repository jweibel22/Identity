using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    public static class TinyUrlResolver
    {
        private static readonly string[] TinyProviders = new[] { "bit.ly", "goo.gl", "ow.ly", "t.co", "TinyUrl", "Tr.im" };

        public static string Resolve(string url)
        {
            if (!TinyProviders.Any(u => url.Contains(u)))
            {
                return url;
            }

            HttpStatusCode statusCode = HttpStatusCode.Moved;

            while (statusCode == HttpStatusCode.Moved)
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    statusCode = response.StatusCode;
                    if (statusCode == HttpStatusCode.Moved)
                    {
                        url = response.Headers["Location"];
                    }
                }
            }

            return url;
        }
    }
}
