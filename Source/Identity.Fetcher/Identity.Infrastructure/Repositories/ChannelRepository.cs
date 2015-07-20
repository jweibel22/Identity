using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain;

namespace Identity.Infrastructure.Repositories
{
    public class ChannelRepository : IDisposable
    {
        private readonly IDbConnection con;

        public ChannelRepository(IDbConnection con)
        {
            this.con = con;
        }

        public Channel GetById(long id)
        {
            return con.Query<Channel>("select * from Channel where Id=@Id", new { Id = id }).SingleOrDefault();
        }

        public IEnumerable<Channel> FindChannelsByName(string name)
        {
            var encoded = "%" + name.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";
            return con.Query<Channel>("select * from Channel where Name like @Name", new { Name = encoded });
        }

        public IEnumerable<Channel> All()
        {
            return con.Query<Channel>("select * from Channel");
        }

        public IEnumerable<long> History(long userId, long channelId)
        {
            return con.Query<long>(@"select ReadHistory.PostId from ReadHistory  
                                    join ChannelItem on ChannelItem.PostId = ReadHistory.PostId and ChannelItem.ChannelId = @ChannelId and ReadHistory.UserId=@UserId", 
                                                                                                                                        new { UserId = userId, ChannelId = channelId });
        }

        public IEnumerable<long> Intersection(long channel1Id, long channel2Id)
        {
            return con.Query<long>(@"select PostId from ChannelItem where ChannelId=@Channel1Id intersect
                                    select PostId from ChannelItem where ChannelId=@Channel2Id ",
                                                                                                new { Channel1Id = channel1Id, Channel2Id = channel2Id });
        }

        public int UnreadCount(long userId, long channelId)
        {
            return con.Query<int>(@"select COUNT(*) from ChannelItem
                                    left join ReadHistory on ChannelItem.PostId = ReadHistory.PostId and ReadHistory.UserId = @UserId
                                    where ChannelItem.ChannelId = @ChannelId and ReadHistory.UserId IS NULL", 
                                                                                                   new { UserId = userId, ChannelId = channelId }).Single();
        }

        public void AddChannel(Channel channel)
        {
            channel.Id = con.Query<int>("insert Channel values (@Name, @Created, @IsPublic); SELECT CAST(SCOPE_IDENTITY() as bigint)", channel).Single();
        }

        public void UpdateChannel(long channelId, bool isPublic, string name, IList<string> rssFeeders)
        {
            con.Execute("update Channel set Name=@Name, IsPublic=@IsPublic where Id=@Id", new{ Id = channelId, IsPublic = isPublic, Name = name});

            RemoveAllFeeders(channelId);

            foreach (var url in rssFeeders)
            {
                FeedInto(RssFeederByUrl(url).Id, channelId);
            }
        }

        public bool PartOf(long channelId, long postId)
        {
            return con.Query<int>("select count(*) from ChannelItem where ChannelId=@ChannelId and PostId=@PostId",
                    new {ChannelId = channelId, PostId = postId}).Single() > 0;
        }

        public void Delete(long channelId)
        {
            con.Execute("delete from Subscription where ChannelId=@Id", new { Id = channelId });
            con.Execute("delete from ChannelOwner where ChannelId=@Id", new { Id = channelId });
            con.Execute("delete from ChannelItem where ChannelId=@Id", new { Id = channelId });
            con.Execute("delete from Channel where Id=@Id", new {Id = channelId});
        }

        public RssFeeder RssFeederById(long id)
        {
            return con.Query<RssFeeder>("select * from RssFeeder where Id=@Id", new {Id= id}).SingleOrDefault();
        }

        public RssFeeder RssFeederByUrl(string url)
        {
            var rssFeeder = con.Query<RssFeeder>("select * from RssFeeder where Url=@Url", new {Url = url}).SingleOrDefault();

            if (rssFeeder == null)
            {
                rssFeeder = new RssFeeder
                {
                    Url = url
                };
                rssFeeder.Id = con.Query<int>("insert RssFeeder values (@Url); SELECT CAST(SCOPE_IDENTITY() as bigint)", rssFeeder).Single();
            }

            return rssFeeder;
        }

        public IEnumerable<RssFeeder> GetRssFeedersForChannel(long channelId)
        {
            return con.Query<RssFeeder>("select RssFeeder.* from RssFeeder join FeedInto on FeedInto.RssFeederId = RssFeeder.Id and FeedInto.ChannelId=@Id", new { Id = channelId });
        }

        public void FeedInto(long rssFeederId, long channelId)
        {
            con.Execute("update FeedInto set ChannelId=@ChannelId where ChannelId=@ChannelId and RssFeederId=@RssFeederId if @@rowcount = 0 insert FeedInto values(@RssFeederId, @ChannelId)", new { ChannelId = channelId, RssFeederId = rssFeederId });            
        }

        public void RemoveAllFeeders(long channelId)
        {
            con.Execute("delete from FeedInto where ChannelId=@ChannelId", new { ChannelId = channelId});            
        }

        public IEnumerable<string> GetRssFeederTags(long rssFeederId)
        {
            return con.Query<string>("select Tag from FeederTags where RssFeederId=@RssFeederId", new {RssFeederId = rssFeederId});
        }

        public void UpdateTagsOfRssFeeder(long rssFeederId, IEnumerable<string> tags)
        {
            con.Execute("delete from FeederTags where RssFeederId=@RssFeederId", new { RssFeederId = rssFeederId });
            foreach (var tag in tags)
            {
                con.Execute("insert FeederTags values(@RssFeederId,@Tag)", new { RssFeederId = rssFeederId, Tag = tag });
            }
        }

        public void Dispose()
        {
            
        }
    }
}
