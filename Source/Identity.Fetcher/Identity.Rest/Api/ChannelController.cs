using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using AutoMapper;
using CsQuery;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using log4net;
using Microsoft.Ajax.Utilities;

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class ChannelController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChannelRepository channelRepo;
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;
        private readonly ILoadDtos dtoLoader;

        private Domain.User user;

        public ChannelController(IDbTransaction transaction, ILoadDtos dtoLoader, ChannelRepository channelRepo, PostRepository postRepo, UserRepository userRepo)
        {
            user = userRepo.FindByName("jimmy");
            this.dtoLoader = dtoLoader;
            this.channelRepo = channelRepo;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
        }

        [HttpGet]
        public IEnumerable<Channel> Get()
        {
            return dtoLoader.LoadChannelList(user, channelRepo.All());    
        }

        [HttpGet]
        public IEnumerable<Channel> Get(string query)
        {
            var channels = channelRepo.FindChannelsByName(query);
            return dtoLoader.LoadChannelList(user, channels);
        }

        [HttpGet]
        public Channel GetById(long id)
        {
            return dtoLoader.LoadChannel(user, channelRepo.GetById(id));           
        }

        [HttpPut]
        public void Subscribe(long id)
        {
            userRepo.Subscribe(user.Id, id);
        }

        [HttpPut]
        public void Unsubscribe(long id)
        {
            userRepo.Unsubscribe(user.Id, id);
        }

        [HttpPut]
        public void Posts(long id, long postId)
        {
            userRepo.Publish(user.Id, id, postId);
        }

        [HttpDelete]
        public void DeletePost(long id, long postId)
        {
            userRepo.Remove(user.Id, id, postId);            
        }

        [HttpPost]
        public Channel Create(Channel channel)
        {
            var x = new Domain.Channel(channel.Name);
            channelRepo.AddChannel(x);
            userRepo.Owns(user.Id, x.Id);

            channel.Id = x.Id;
            channel.IsPrivate = true;
            return channel;
        }

        [HttpPut]
        public Channel Update(long id, Channel channel)
        {
            channelRepo.UpdateChannel(id, !channel.IsPrivate, channel.Name, channel.RssFeeders.Select(f => f.Url).ToList());

            return dtoLoader.LoadChannel(user, channelRepo.GetById(id));
        }

        [HttpDelete]
        public void Delete(long id)
        {
            channelRepo.Delete(id);
        }

        [HttpPost]
        public Post Posts(long id, Post post)
        {
            var p = new Domain.Post
            {
                Created = post.Created,
                Description = post.Description,
                Title = post.Title,
                Uri = post.Uri
            };

            if (p.Uri != null && p.Title == null)
            {
                PopulateMetaData(p);
            }

            postRepo.AddPost(p);
            userRepo.Publish(user.Id, user.SavedChannel, p.Id);
            postRepo.TagPost(p.Id, post.Tags);

            return post;
        }

        private void PopulateMetaData(Domain.Post p)
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

        [HttpGet]
        public Channel Get(long id, bool onlyUnread, DateTime timestamp, int fromIndex, string orderBy)
        {          
           log.Debug("Fetching items from channel " + id + " and page " + fromIndex);

            var channel = channelRepo.GetById(id);

            if (channel == null)
            {
                return null;
            }

            var owns = userRepo.Owns(user.Id);

            var result = dtoLoader.LoadChannel(user, channel);
            result.Posts = dtoLoader.LoadChannelPosts(user, channel, !owns.Any(c => c.Id == id) && onlyUnread, timestamp, fromIndex, orderBy).ToList();

            log.Debug("Items from channel " + id + " was fetched");

            return result;
        }
    }
}
