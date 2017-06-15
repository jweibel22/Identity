using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Reddit;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using log4net;
using Microsoft.WindowsAzure.Storage;

namespace FeederJob2
{
    class NlpTest
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Log(string s)
        {
            Console.WriteLine(String.Format("{0}. {1}", DateTime.Now.ToString("HH:mm:ss.fff"), s));
        }


        static SuggestedSubReddits NewSuggest(RedditIndex index, SubRedditOccurences sro, NLPHelper helper, long postId, IEnumerable<Entity> article)
        {
            var entities = article.Where(e => !helper.CommonWords.ContainsKey(e.EntityName)).ToList();

            IList<Text> texts = new List<Text>();

            foreach (var entity in entities)
            {
                log.Info("Adding " + entity.EntityName);
                texts.Add(sro.Add(entity.EntityName, entity.EntityType));
                log.Info(entity.EntityName + " added");
            }

            log.Info("Looking up occurences: " + String.Join(", ", texts.Select(t => t.Id)));
            var xx = sro.FindOccurences(texts.Select(t => t.Id)).ToList();
            log.Info("Occurences found");

            var subredditPostCounts = index.SubReddits.ToDictionary(sr => sr.Id, sr => sr.PostCount);
            var subredditNames = index.SubReddits.ToDictionary(sr => sr.Id, sr => sr.Name);

            var totalRedditPosts = index.TotalPostCount;
            var logTotalRedditPosts = Math.Log(index.TotalPostCount);
            var totalCounts = xx.GroupBy(x => x.TextId).ToDictionary(x => x.Key, x => x.Sum(ff => ff.Count));
            var weighted = xx
                            .GroupBy(x => x.SubRedditId)
                            .Select(g => new SubRedditScore { Id = g.Key, Score = g.Sum(a => (Math.Log(a.Count) / Math.Log(subredditPostCounts[a.SubRedditId])) / (Math.Log(totalCounts[a.TextId]) / logTotalRedditPosts)) / (texts.Count) });

            //			var temp = xx.Select(x => new Temp
            //			{
            //				Id = x.SubRedditId,
            //				EntityName = texts.Single(e => e.Id == x.TextId).Content,
            //				Name = subredditNames[x.SubRedditId],
            //				Occurences = x.Count,
            //				PostCount = subredditPostCounts[x.SubRedditId],
            //				TotalEntityOccurences = totalCounts[x.TextId],
            //				TotalRedditPosts = totalRedditPosts
            //			}).Where(x => x.LogSubRedditFreq > 0.2).OrderBy(x => x.Name).ThenByDescending(x => x.Score).ToList();
            //	temp.Dump();

            log.Info("Suggestions found");

            return new SuggestedSubReddits
            {
                ArticleId = postId,
                TopSubReddits = weighted.OrderByDescending(f => f.Score).Take(5).ToList()
            };
        }

        private static IEnumerable<Entity> GetEntities(SqlTransaction con, long postId)
        {
            var sql = @"select r.NLPEntityId as EntityId, e.Name as EntityName, e.Type as EntityType from HNArticleEntities r join NLPEntities e on e.Id = r.NLPEntityId where r.HNArticleId = @PostId";

            return con.Connection.Query<Entity>(sql, new {PostId = postId}, con).ToList();
        }

        private static IEnumerable<Post> GetPosts(SqlTransaction con)
        {
            var sql = @"select top 10 p.Id, p.Title from ChannelItem ci join Post p on p.Id = ci.PostId where ci.ChannelId = 118 and p.Id >= 238197 order by p.Id";

            return con.Connection.Query<Post>(sql, new { }, con).ToList();
        }

        static void PrintToCsv(IDictionary<long, string> articleNames, IDictionary<long, string> subRedditNames, IEnumerable<SuggestedSubReddits> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                var tags = suggestion.TopSubReddits;
                Func<int, string> gg = i =>
                {
                    var d = i < tags.Count ? subRedditNames[tags[i].Id] : null;
                    return d == null ? "" : d;
                };

                Func<int, double> score = i =>
                {
                    return i < tags.Count ? tags[i].Score : 0;
                };

                Console.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}", articleNames[suggestion.ArticleId].Trim(), gg(0), gg(1), gg(2), gg(3), gg(4), score(0), score(1), score(2), score(3), score(4));
            }
        }

        public static void Run()
        {
            var commonWordsFile = @"C:\git\Identity\Source\Identity.Fetcher\Identity.Infrastructure\commonwords-english.txt";
            var commonNounsFile = @"C:\transit\RedditPosts\nouns.csv";
            var helper = new NLPHelper(commonWordsFile, commonNounsFile);

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            if (!CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=jweibel;AccountKey=mGw3mQxyZ9F2NHW8WHAK2qtwapm5PxvpfpUJlXNMaE0mXdzhV43QwTG3aUQGrtoSnEpJMSXap2dzi1wmiMQv/w==", out cloudStorageAccount))
            {
                throw new Exception("aaaa");
            }


            using (SqlConnection con = new SqlConnection(@"Data Source=v5kf9wt87u.database.windows.net;Initial Catalog=Identity;Persist Security Info=True;User ID=jweibel;Password=MiInPw01;Connect Timeout=30"))
            {
                con.Open();

                using (var trx = con.BeginTransaction())
                {

                    var postRepo = new PostRepository(trx);
                    //var posts = postRepo.PostsFromChannel(4, 118, 0, "Added", 1).ToList();
                    //var postIds = Enumerable.Range(0, 10).Cast<long>();
                    //var posts = postRepo.GetByIds(postIds, 4);
                    var posts = GetPosts(trx);
                    //var posts = new[] { p}.ToList();
                    var subRedditNames = con.Query("select id, name from RedditIndex_SubReddit", new { }, trx).Cast<IDictionary<string, object>>().ToDictionary(row => (long)row["id"], row => (string)row["name"]);
                    var articleNames = posts.ToDictionary(g => g.Id, g => g.Title);

                    var directoryFactory = new LuceneDirectoryFactory(cloudStorageAccount, @"c:\transit\RedditPosts\Indexes");
                    var repo = new RedditIndexRepository(trx);
                    var index = repo.GetRedditIndex(4);
                    var service = new SubRedditOccurences(repo, index, directoryFactory);
                    var suggestions = posts.Select(post => NewSuggest(index, service, helper, post.Id, GetEntities(trx, post.Id))).ToList();


                    log.Info("Printing result");
                    PrintToCsv(articleNames, subRedditNames, suggestions);

                    //trx.Commit();                    
                }
            }
        }
    }
}
