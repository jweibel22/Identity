using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Domain.Events;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Feeders;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Reddit;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services.AutoTagger;
using Identity.Infrastructure.Services.NLP;
using log4net;
using log4net.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Post = Identity.Domain.Post;
using Text = Identity.Infrastructure.Services.NLP.Text;

namespace FeederJob2
{
    class PostWithTitle
    {
        public long PostId { get; set; }

        public string Title { get; set; }
    }


    class APost
    {
        public long Id { get; set; }

        public string Title { get; set; }
    }

    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //static void Main()
        //{
        //    XmlConfigurator.Configure();

        //    Functions.SyncFeeds("", TextWriter.Null);
        //    Functions.ReloadOntology();

        //    if (DateTime.Now.Hour == 0)
        //    {
        //        Functions.RefreshChannelScores();
        //    }
        //}

        static void AddToIndex(NLPEntityRepository nlpRepo, SubRedditOccurences service,
            Dictionary<int, List<Identity.Domain.Post>> allPosts)
        {
            var allEntities = nlpRepo.Entities();
            var allEntityRelations = nlpRepo.EntitiesInPosts();
            Func<long, IEnumerable<NLPEntity>> f = postId =>
                from r in allEntityRelations
                join e in allEntities on r.Value equals e.Id
                where r.Key == postId
                select e;

            var entitiesToAdd = allPosts.SelectMany(kv => kv.Value).SelectMany(post => f(post.Id)).ToList();

            var count = entitiesToAdd.Count;
            var idx = 0;

            foreach (var entity in entitiesToAdd)
            {
                try
                {
                    service.Add(entity.Name, entity.Type);
                    idx++;
                }
                catch (Exception ex)
                {
                    log.Error("Unable to add entity with name " + entity.Name, ex);
                }

            }
        }

        static IEnumerable<Submission> LoadFromFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                Console.WriteLine("");
                Console.WriteLine("Processing " + file);
                foreach (var x in Submission.LoadFromCsvFile(file))
                {
                    yield return x;
                }
            }
        }




        static void Processing(int i)
        {
            if (i % 10 == 0)
            {
                if (i % 1000 == 0)
                {
                    Console.WriteLine(".");
                }
                else
                {
                    Console.Write(".");
                }
            }
        }

        static PostAndId Parse(string s)
        {
            var line = s.Split(';');
            long postId;
            long channelId;
            if (line.Length == 3 && Int64.TryParse(line[0], out postId) && Int64.TryParse(line[2], out channelId))
            {
                return new PostAndId
                {
                    PostId = postId,
                    Title = line[1].Trim(),
                    Id = channelId
                };
            }
            else
            {
                return null;
            }
        }


        static void Main()
        {
            XmlConfigurator.Configure();

            var commonWordsFile = @"C:\git\Identity\Source\Identity.Fetcher\Identity.Infrastructure\commonwords-english.txt";
            var commonNounsFile = @"C:\transit\RedditPosts\nouns.csv";

            var helper = new EnglishLanguage(commonWordsFile, commonNounsFile);
            //var google = new GoogleNLPClient("AIzaSyBAPJ3LgmXm-DrmG6CiZ6AHslkMl8C999U", "https://language.googleapis.com/v1/documents:analyzeEntities", helper);

            //var es = google.Get(new[] { new Text { Content = "An Infographic: How Bad Is U.S. Health Care?" } });


            var options = new WordExtractionOptions
            {
                IgnoreCommonWords = true,
                RemovePunctuation = true,
                Stem = false
            };
            var wordExtractor = new WordExtractor(commonWordsFile, commonNounsFile, options);

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            if (!CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=jweibel;AccountKey=mGw3mQxyZ9F2NHW8WHAK2qtwapm5PxvpfpUJlXNMaE0mXdzhV43QwTG3aUQGrtoSnEpJMSXap2dzi1wmiMQv/w==", out cloudStorageAccount))
            {
                throw new Exception("aaaa");
            }

            //IList<string> posts = null;

            //using (SqlConnection con = new SqlConnection(@"Data Source=v5kf9wt87u.database.windows.net;Initial Catalog=Identity;Persist Security Info=True;User ID=jweibel;Password=MiInPw01;Connect Timeout=30"))
            //{
            //    con.Open();
            //    using (var trx = con.BeginTransaction())
            //    {
            //        var postRepo = new PostRepository(trx);
            //        posts = postRepo.FindTrainingPostsTitles().ToList();// .XXX(30141, 0, 600000).ToList();
            //    }
            //}
            var inputChannelMap = new Dictionary<long, int>
            {
                {30141, 0},
                {30133, 1},
                {30136, 2},
                {30122, 3},
                {30118, 4},
                {30140, 5},
            };



            //IList<PostAndId> trainingPosts;
            //IList<PostWithTitle> texts;
            //IList<PostAndId> textsInPosts;

            //trainingPosts = File
            //                .ReadAllLines(@"C:\transit\RedditPosts\TrainingPosts.csv")
            //                .Select(Parse)
            //                .Where(x => x != null)
            //                .Select(p => new PostAndId { PostId = p.PostId, Title = p.Title, Id = inputChannelMap[p.Id] })
            //                .ToList();

            //texts = File
            //               .ReadAllLines(@"C:\transit\RedditPosts\Texts.csv")
            //               .Select(line => line.Split(';'))
            //               .Select(line => new PostWithTitle { PostId = Int64.Parse(line[0]), Title = line[1].Trim() })
            //               .ToList();
            //textsInPosts = File
            //                   .ReadAllLines(@"C:\transit\RedditPosts\TextsInPosts.csv")
            //                   .Select(line => line.Split(';'))
            //                   .Select(line => new PostAndId { PostId = Int64.Parse(line[0]), Id = Int64.Parse(line[1]) })
            //                   .ToList();

            //var xxx = from tip in textsInPosts
            //          join t in texts on tip.Id equals t.PostId
            //          join p in trainingPosts on tip.PostId equals p.PostId
            //          select t.Title;

            //var relevantTexts = xxx.Distinct().ToList();

            var relevantTexts = File.ReadAllLines(@"C:\transit\RedditPosts\labels.csv")
                .Take(1000)
                .Select(line => line.Split(';'))
                .Where(line => Int32.Parse(line[2]) != 0)
                .SelectMany(line => wordExtractor.GetWords(line[1]))
                .Distinct()
                .ToList();
                    


            var connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Reddit;Trusted_Connection=True;";
            //
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (var trx = con.BeginTransaction())
                {

                    //var subRedditNames = con.Query("select id, name from RedditIndex_SubReddit", new { }, trx).Cast<IDictionary<string, object>>().ToDictionary(row => (long)row["id"], row => (string)row["name"]);
                    //var articleNames = posts.ToDictionary(g => g.Id, g => g.Title);

                    var directoryFactory = new LuceneDirectoryFactory(cloudStorageAccount, @"c:\transit\RedditPosts\Indexes");
                    var repo = new RedditIndexRepository(trx);
                    var index = repo.GetRedditIndex(1);


                    //var factory = new RedditIndexFactory(repo, directoryFactory);
                    //var files = new[] { "RS_2010-01.csv", "RS_2011-01.csv", "RS_2012-01.csv", "RS_2013-01.csv", "RS_2014-01.csv", "RS_2015-01.csv", "RS_2016-10.csv", "RS_2016-12.csv" };
                    //var submissions = LoadFromFiles(files.Select(f => Path.Combine(@"C:\transit\RedditPosts\Input", f)));
                    //factory.Build(IndexStorageLocation.Local, submissions);


                    var service = new SubRedditOccurences(repo, index, directoryFactory);

                    //var allWords = posts.SelectMany(p => wordExtractor.GetWords(p)).Distinct().ToList();
                    var idx = 0;

                    foreach (var word in relevantTexts)
                    {
                        try
                        {
                            service.Add(word, "");
                            idx++;
                            Processing(idx);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    trx.Commit();

                    //0; other
                    //1:programming
                    //2; politics
                    //3; technology;
                    //4; science
                    //5; history
                    //6:art

                }
            }

        }

    }
}
