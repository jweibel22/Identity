using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using log4net;

namespace Identity.Infrastructure.WebScrapers
{
    public class WebScraperJob
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConnectionFactory connectionFactory;
        private const string rssFeederUsername = "rssfeeder";
        private const int GhostChannelId = 22;

        public WebScraperJob(ConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public void Run()
        {
            using (var session = connectionFactory.NewTransaction())
            {
                var channelRepo = new ChannelRepository(session);
                var userRepo = new UserRepository(session);
                var postRepo = new PostRepository(session);

                var rssFeederUser = userRepo.FindByName(rssFeederUsername);

                if (rssFeederUser == null)
                {
                    log.Error("User 'rssFeeder' was not found");
                    return;
                }

                var scrapers = channelRepo.OutOfSyncWebScrapers(TimeSpan.FromHours(1));

                foreach (var scraper in scrapers)
                {
                    var scraperAlgorithm = new AlScraper(scraper.Url);

                    var articles = scraperAlgorithm.Scrape().ToList();

                    int importCount = 0;

                    foreach (var article in articles)
                    {
                        if (postRepo.WebScraperItemAlreadyPosted(article.Title, article.Updated, scraper.Id)) return;

                        var post = postRepo.GetByUrl(article.Url);

                        if (post == null)
                        {
                            post = new Post
                            {
                                Created = article.Updated,
                                Description = article.Text,
                                Title = article.Title,
                                Uri = article.Url
                            };

                            postRepo.AddPost(post, false);
                            postRepo.TagPost(post.Id, article.Tags);
                        }
                        else
                        {
                            var tags = postRepo.Tags(post.Id).Select(t => t.Tag).ToList();
                            var missingTags = article.Tags.Any(tag => !tags.Contains(tag));
                            
                            if (missingTags)
                            {
                                var allTags = tags.Union(article.Tags).Distinct().ToList();
                                postRepo.TagPost(post.Id, allTags);
                            }
                        }

                        postRepo.AddWebScraperItem(scraper.Id, post.Id, article.Updated);
                        userRepo.Publish(rssFeederUser.Id, GhostChannelId, post.Id);

                        log.Info(String.Format("Imported {0} of {1} articles", ++importCount, articles.Count));
                    }
                }

                session.Commit();
            }
        }
    }
}
