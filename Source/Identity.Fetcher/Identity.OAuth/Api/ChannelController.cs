﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using CsQuery;
using Identity.Domain;
using Identity.Domain.Events;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.OAuth;
using Identity.OAuth.EventHandlers;
using log4net;
using Channel = Identity.Infrastructure.DTO.Channel;
using ChannelDisplaySettings = Identity.Infrastructure.DTO.ChannelDisplaySettings;
using Post = Identity.Infrastructure.DTO.Post;

namespace Identity.Rest.Api
{
    [Authorize]
    [UnitOfWorkCommit]
    public class ChannelController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChannelRepository channelRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly ChannelLinkRepository channelLinkRepo;
        private readonly ILoadDtos dtoLoader;
        private readonly Bus bus;
        private readonly ChannelLinkEventBatch eventBatch;

        private PostListLoader postListLoader;
        private Domain.User user;

        public ChannelController(ILoadDtos dtoLoader, ChannelRepository channelRepo, PostRepository postRepo,
            UserRepository userRepo, Bus bus, PostListLoader postListLoader, ChannelLinkRepository channelLinkRepo, ChannelLinkEventBatch eventBatch)
        {
            var identity = User.Identity as ClaimsIdentity;
            user = userRepo.FindByName(identity.Name);

            this.dtoLoader = dtoLoader;
            this.channelRepo = channelRepo;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.bus = bus;
            this.postListLoader = postListLoader;
            this.channelLinkRepo = channelLinkRepo;
            this.eventBatch = eventBatch;
        }

        [HttpGet]
        public IEnumerable<Channel> Get()
        {
            return dtoLoader.LoadChannelList(user, channelRepo.AllPublic());    
        }

        [HttpGet]
        public IEnumerable<ChannelListItem> Get(string query)
        {
            var channels = channelRepo.FindPublicChannelsByName(query);

            return channels.Select(c => new ChannelListItem
            {
                Name = c.Name,
                Id = c.Id
            });
        }

        [HttpGet]
        public Channel GetById(long id)
        {
            return dtoLoader.LoadChannel(user, channelRepo.GetById(id));           
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage Rss(long id)
        {
            var channel = Mapper.Map<Channel>(channelRepo.GetById(id));
            var rssFeeder = userRepo.FindByName("rssfeeder");

            var namedPostList = new NamedPostList
            {
                Name = channel.Name,
                Posts = Enumerable.Range(0, 5)
                    .SelectMany(i => postRepo.PostsFromChannel(rssFeeder.Id, channel.Id, i*30, "Added",30, true))
                    .Select(Mapper.Map<Post>)
                    .ToList()
            };
            
            return Request.CreateResponse(HttpStatusCode.OK, namedPostList, new SyndicationFeedFormatter());
        }

        [HttpPut]
        public void Leave(long id)
        {
            userRepo.Leave(user.Id, id);
            eventBatch.Add(new UserLeaves { ChannelId = id, UserId = user.Id });
        }

        [HttpPut]
        public void Grant(long id, long userId)
        {
            userRepo.Grant(userId, id);
            eventBatch.Add(new UserJoins { ChannelId = id, UserId = user.Id });
        }

        [HttpPut]
        public void Posts(long id, long postId)
        {
            userRepo.Publish(user.Id, id, postId);
            eventBatch.Add(new PostAdded { ChannelId = id, PostId = postId });
        }

        [HttpDelete]
        public void DeletePost(long id, long postId)
        {
            userRepo.Remove(user.Id, id, postId);
            eventBatch.Add(new PostRemoved { ChannelId = id, PostId = postId });
        }

        [HttpPost]
        public Channel Create(Channel channel)
        {
            var x = Domain.Channel.New(channel.Name);
            channelRepo.AddChannel(x);
            userRepo.Owns(user.Id, x.Id, false);

            channel.Id = x.Id;
            channel.IsPrivate = true;

            eventBatch.Add(new ChannelAdded { ChannelId = channel.Id });
            eventBatch.Add(new UserJoins { ChannelId = channel.Id, UserId = user.Id });
            return channel;
        }

        [HttpPut]
        public Channel Update(long id, Channel channel)
        {
            var c = channelRepo.GetById(channel.Id);

            c.IsPublic = !channel.IsPrivate;
            c.Name = channel.Name;

            var existingSubscriptions = channelRepo.GetSubscriptions(channel.Id);
            channelRepo.UpdateChannel(c, channel.Subscriptions.Select(x => x.Id));

            foreach (var removed in existingSubscriptions.Select(s => s.Id).Except(channel.Subscriptions.Select(x => x.Id)))
            {
                eventBatch.Add(new SubscriptionRemoved { UpstreamChannelId = removed, DownstreamChannelId = id });
            }
            foreach (var added in channel.Subscriptions.Select(x => x.Id).Except(existingSubscriptions.Select(s => s.Id)))
            {
                eventBatch.Add(new SubscriptionAdded { UpstreamChannelId = added, DownstreamChannelId = id });
            }

            return dtoLoader.LoadChannel(user, channelRepo.GetById(id));
        }

        [HttpPut]
        public void RemoveSubscription(long id, long childId)
        {
            channelRepo.RemoveSubscription(id, childId);
            eventBatch.Add(new SubscriptionRemoved { UpstreamChannelId = childId, DownstreamChannelId = id });
        }

        [HttpPut]
        public void AddSubscription(long id, long childId)
        {
            channelRepo.AddSubscription(id, childId);

            if (channelLinkRepo.CyclesExist(id))
            {
                throw new ApplicationException("Unable to add subscription because it would introduce a cycle");
            }

            eventBatch.Add(new SubscriptionAdded { UpstreamChannelId = childId, DownstreamChannelId = id });
        }

        [HttpPut]
        public void AddFeed(long id, string url, FeedType type)
        {
            var feed = channelRepo.GetFeed(url, type);
            channelRepo.AddSubscription(id, feed.ChannelId);
            eventBatch.Add(new SubscriptionAdded { UpstreamChannelId = feed.ChannelId, DownstreamChannelId = id });
            bus.Publish("new-feeder-created", feed.Id.ToString());
        }

        [HttpDelete]
        public void Delete(long id)
        {
            channelRepo.Delete(user.Id, id);
            eventBatch.Add(new ChannelDeleted { ChannelId = id });
        }

        [HttpPost]
        public Post Posts(long id, Post post)
        {
            var p = postRepo.GetByUrl(post.Uri);

            if (p == null)
            {
                p = new Domain.Post
                {
                    Created = post.Created,
                    Description = post.Description ?? "",
                    Title = post.Title,
                    Uri = post.Uri
                };

                if (p.Uri != null && String.IsNullOrEmpty(p.Title))
                {
                    PopulateMetaData(p);
                }

                postRepo.AddPost(p, false);                
            }
            
            userRepo.Publish(user.Id, id, p.Id);
            eventBatch.Add(new PostAdded { ChannelId = id, PostId = p.Id });

            if (post.Tags != null)
            {
                postRepo.TagPost(p.Id, post.Tags);    
            }            

            return Mapper.Map<Post>(p);
        }

        [HttpPut]
        public void MarkAllAsRead(int id)
        {
            channelRepo.MarkAllAsRead(user.Id, id);
        }

        private void PopulateMetaData(Domain.Post p)
        {
            try
            {
                var req = WebRequest.CreateHttp(p.Uri);
                using (var stream = req.GetResponse().GetResponseStream())
                {
                    CQ dom = new StreamReader(stream).ReadToEnd();
                    p.Title = dom["title"].Text();

                    var descr1 = dom["meta[name=description]"];

                    if (descr1.Elements.Any())
                    {
                        if (descr1.Attr("content") != null)
                        {
                            p.Description = descr1.Attr("content");
                        }
                        else if (descr1.Attr("value") != null)
                        {
                            p.Description = descr1.Attr("value");
                        }
                    }
                    else
                    {
                        var descr2 = dom["meta[itemprop=description]"];

                        if (descr2.Elements.Any() && descr2.Attr("content") != null)
                        {
                            p.Description = descr2.Attr("content");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Unable to extract meta data from link", ex);

                if (p.Title == null)
                {
                    p.Title = p.Uri;                    
                }                
            }
        }

        [HttpGet]
        public IList<Post> Get(long id, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy, int pageSize)
        {          
            return postListLoader.Load(id, user, onlyUnread, timestamp, fromIndex, orderBy, pageSize);
        }

        [HttpPut]
        public void UpdateDisplaySettings(long id, long userId, ChannelDisplaySettings settings)
        {
            channelRepo.UpdateChannelDisplaySettings(userId, id, Mapper.Map<Domain.ChannelDisplaySettings>(settings));
        }
    }
}
