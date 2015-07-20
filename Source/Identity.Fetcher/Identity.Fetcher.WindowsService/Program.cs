using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Fetcher.WindowsService.Rss;
using Identity.Infrastructure.Repositories;

namespace Identity.Fetcher.WindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            IDbConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
            var channelRepo = new ChannelRepository(con);
            var postRepo = new PostRepository(con);
            var userRepo = new UserRepository(con);

            var feedRefresher = new RssFeedRefresher(channelRepo, postRepo, userRepo);

            feedRefresher.RefreshFeeds();

            Console.ReadLine();
        }
    }
}
