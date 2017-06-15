using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Identity.Domain;
using Identity.Domain.Events;
using Identity.Infrastructure.Feeders.FeedReaders;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services.NLP;

namespace Identity.Infrastructure.Feeders
{
    public class FeedRefresher
    {
        private readonly ConnectionFactory connectionFactory;
        private readonly FeedReaderFactory _feedReaderFactory;
        private readonly Logger log;
        private readonly EnglishLanguage language;
        private readonly GoogleNLPClient nlpClient;

        public FeedRefresher(ConnectionFactory connectionFactory, TextWriter azureLog, EnglishLanguage language, GoogleNLPClient nlpClient)
        {
            this.connectionFactory = connectionFactory;
            this.language = language;
            this.nlpClient = nlpClient;
            this._feedReaderFactory = new FeedReaderFactory();
            log = new Logger(azureLog);            
        }
        
        public void Run(User rssFeederUser, IEnumerable<Feed> feeders)
        {
            ChannelLinkGraph graph;
            var events = new ChannelLinkEventListener();

            using (var session = connectionFactory.NewTransaction())
            {
                var channelLinkRepo = new ChannelLinkRepository(session.Transaction);
                graph = channelLinkRepo.GetGraph();
            }

            var fetchFeedItemsBlock = new TransformBlock<Feed, Tuple<Feed, IEnumerable<FeedItem>>>(rssFeeder =>
            {
                log.Info("fetching feed " + rssFeeder.Url);
                try
                {
                    var rssReader = _feedReaderFactory.GetReader(rssFeeder);
                    var feed = rssReader.Fetch(rssFeeder.Url);
                    return new Tuple<Feed, IEnumerable<FeedItem>>(rssFeeder, feed);
                }
                catch (Exception ex)
                {
                    log.Error("Unable to fetch feed " + rssFeeder.Url, ex);
                    return new Tuple<Feed, IEnumerable<FeedItem>>(rssFeeder, new List<FeedItem>());
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });

            var storeFeedItemBlock = new ActionBlock<Tuple<Feed, IEnumerable<FeedItem>>>(t =>
            {
                if (!t.Item2.Any())
                {
                    return;
                }

                using (var session = connectionFactory.NewTransaction())
                {
                    try
                    {
                        var analyzer = new PostNlpAnalyzer(new NLPEntityRepository(session.Transaction), language, nlpClient);
                        var processor = new FeedProcessor(log, new ChannelRepository(session.Transaction),
                            new UserRepository(session.Transaction),new PostRepository(session.Transaction), analyzer);

                        processor.ProcessFeed(rssFeederUser, t.Item1, events, t.Item2);   
                        session.Commit();

                        log.Info("Done processing feed items from feed " + t.Item1.Url);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unable store feed items from feed " + t.Item1.Url, ex);
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });

            fetchFeedItemsBlock.LinkTo(storeFeedItemBlock);
            
            fetchFeedItemsBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)storeFeedItemBlock).Fault(t.Exception);
                else storeFeedItemBlock.Complete();
            });
   
            foreach (var feeder in feeders)
            {
                fetchFeedItemsBlock.Post(feeder);
            }

            fetchFeedItemsBlock.Complete();

            storeFeedItemBlock.Completion.Wait();

            log.Info("Refreshing unread counts");

            using (var session = connectionFactory.NewTransaction())
            {
                var repo = new ChannelLinkRepository(session.Transaction);
                var dirtyUserChannels = graph.ApplyChanges(events.Events);

                foreach (var edge in dirtyUserChannels.Channels)
                {
                    repo.UpdateUnreadCounts(edge.To.Id, edge.From.Id);
                }

                session.Commit();
            }
        }
    }
}
