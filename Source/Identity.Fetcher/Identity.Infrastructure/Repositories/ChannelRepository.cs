using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain;
using Channel = Identity.Domain.Channel;
using RssFeeder = Identity.Domain.RssFeeder;

namespace Identity.Infrastructure.Repositories
{
    public class ChannelRepository : IDisposable
    {
        private readonly IDbTransaction con;

        public ChannelRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public Channel GetById(long id)
        {
            return con.Connection.Query<Channel>("select * from Channel where Id=@Id", new { Id = id }, con).SingleOrDefault();
        }

        public IEnumerable<Channel> FindChannelsByName(string name)
        {
            var encoded = "%" + name.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";
            return con.Connection.Query<Channel>("select * from Channel where Name like @Name", new { Name = encoded }, con);
        }

        public IEnumerable<Channel> All()
        {
            return con.Connection.Query<Channel>("select * from Channel", null, con);
        }

        public IEnumerable<WeightedTag> GetTagCloud(long channelId)
        {
            return con.Connection.Query<WeightedTag>(@"select top 20 count(*) as Weight, Tag as Text from Tagged join ChannelItem ci on ci.PostId = Tagged.PostId and ci.ChannelId = @ChannelId
                                            group by Tag order by COUNT(*) desc", new { ChannelId = channelId }, con);
        }

        public IEnumerable<long> History(long userId, long channelId)
        {
            return con.Connection.Query<long>(@"select ReadHistory.PostId from ReadHistory  
                                    join ChannelItem on ChannelItem.PostId = ReadHistory.PostId and ChannelItem.ChannelId = @ChannelId and ReadHistory.UserId=@UserId", 
                                                                                                                                        new { UserId = userId, ChannelId = channelId }, con);
        }

        public IEnumerable<long> Intersection(long channel1Id, long channel2Id)
        {
            return con.Connection.Query<long>(@"select PostId from ChannelItem where ChannelId=@Channel1Id intersect
                                    select PostId from ChannelItem where ChannelId=@Channel2Id ",
                                                                                                new { Channel1Id = channel1Id, Channel2Id = channel2Id }, con);
        }

        public int UnreadCount(long userId, long channelId)
        {
            return con.Connection.Query<int>(@"select COUNT(*) from ChannelItem
                                    left join ReadHistory on ChannelItem.PostId = ReadHistory.PostId and ReadHistory.UserId = @UserId
                                    where ChannelItem.ChannelId = @ChannelId and ReadHistory.UserId IS NULL", 
                                                                                                   new { UserId = userId, ChannelId = channelId }, con).Single();
        }

        public void AddChannel(Channel channel)
        {
            channel.Id = con.Connection.Query<int>("insert Channel values (@Name, @Created, @IsPublic); SELECT CAST(SCOPE_IDENTITY() as bigint)", channel, con).Single();
        }

        public void UpdateChannel(long channelId, bool isPublic, string name, IList<string> rssFeeders)
        {
            con.Connection.Execute("update Channel set Name=@Name, IsPublic=@IsPublic where Id=@Id", new { Id = channelId, IsPublic = isPublic, Name = name }, con);

            RemoveAllFeeders(channelId);

            foreach (var url in rssFeeders)
            {
                FeedInto(RssFeederByUrl(url).Id, channelId);
            }
        }

        public bool PartOf(long channelId, long postId)
        {
            return con.Connection.Query<int>("select count(*) from ChannelItem where ChannelId=@ChannelId and PostId=@PostId",
                    new {ChannelId = channelId, PostId = postId}, con).Single() > 0;
        }

        public void Delete(long userId, long channelId)
        {
            var cnt = con.Connection.Query<int>("select count(*) from ChannelOwner where UserId=@UserId and ChannelId=@ChannelId and IsLocked=false",
                new{ UserId = userId, ChannelId = channelId }).Single();

            if (cnt == 0)
            {
                throw new Exception("Access denied, you're not allowed to delete this channel");
            }
            
            con.Connection.Execute("delete from Subscription where ChannelId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from ChannelOwner where ChannelId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from ChannelItem where ChannelId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from Channel where Id=@Id", new { Id = channelId }, con);
        }

        public RssFeeder RssFeederById(long id)
        {
            return con.Connection.Query<RssFeeder>("select * from RssFeeder where Id=@Id", new { Id = id }, con).SingleOrDefault();
        }

        public RssFeeder RssFeederByUrl(string url)
        {
            var rssFeeder = con.Connection.Query<RssFeeder>("select * from RssFeeder where Url=@Url", new { Url = url }, con).SingleOrDefault();

            if (rssFeeder == null)
            {
                rssFeeder = new RssFeeder
                {
                    Url = url
                };
                rssFeeder.Id = con.Connection.Query<int>("insert RssFeeder values (@Url,NULL); SELECT CAST(SCOPE_IDENTITY() as bigint)", rssFeeder, con).Single();
            }

            return rssFeeder;
        }

        public IEnumerable<RssFeeder> AllRssFeeders()
        {
            return con.Connection.Query<RssFeeder>("select RssFeeder.* from RssFeeder", null, con);
        }

        public IEnumerable<RssFeeder> OutOfSyncRssFeeders(TimeSpan timeSpan)
        {
            return con.Connection.Query<RssFeeder>("select RssFeeder.* from RssFeeder where LastFetch is null or DATEDIFF(mi, LastFetch, @Now) >= @TotalMinutes", new { DateTimeOffset.Now, timeSpan.TotalMinutes }, con);
        }

        public void UpdateRssFeeder(RssFeeder rssFeeder)
        {
            con.Connection.Execute("update RssFeeder set LastFetch=@LastFetch, Url=@Url where Id=@Id", rssFeeder, con);
        }

        public IEnumerable<RssFeeder> GetRssFeedersForChannel(long channelId)
        {
            return con.Connection.Query<RssFeeder>("select RssFeeder.* from RssFeeder join FeedInto on FeedInto.RssFeederId = RssFeeder.Id and FeedInto.ChannelId=@Id", new { Id = channelId }, con);
        }

        public IEnumerable<long> GetChannelsForRssFeeder(long rssFeederId)
        {
            return con.Connection.Query<long>("select ChannelId from FeedInto where RssFeederId = @RssFeederId", new { RssFeederId = rssFeederId }, con);
        }

        public void FeedInto(long rssFeederId, long channelId)
        {
            con.Connection.Execute("update FeedInto set ChannelId=@ChannelId where ChannelId=@ChannelId and RssFeederId=@RssFeederId if @@rowcount = 0 insert FeedInto values(@RssFeederId, @ChannelId)", new { ChannelId = channelId, RssFeederId = rssFeederId }, con);            
        }

        public void RemoveAllFeeders(long channelId)
        {
            con.Connection.Execute("delete from FeedInto where ChannelId=@ChannelId", new { ChannelId = channelId }, con);            
        }

        public IEnumerable<string> GetRssFeederTags(long rssFeederId)
        {
            return con.Connection.Query<string>("select Tag from FeederTags where RssFeederId=@RssFeederId", new { RssFeederId = rssFeederId }, con);
        }

        public void UpdateTagsOfRssFeeder(long rssFeederId, IEnumerable<string> tags)
        {
            con.Connection.Execute("delete from FeederTags where RssFeederId=@RssFeederId", new { RssFeederId = rssFeederId }, con);
            foreach (var tag in tags)
            {
                con.Connection.Execute("insert FeederTags values(@RssFeederId,@Tag)", new { RssFeederId = rssFeederId, Tag = tag }, con);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
