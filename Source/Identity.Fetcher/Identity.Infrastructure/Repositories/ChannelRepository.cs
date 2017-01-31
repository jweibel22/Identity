using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Identity.Domain;
using Identity.Infrastructure.Helpers;
using Channel = Identity.Domain.Channel;
using Newtonsoft.Json;
using ChannelDisplaySettings = Identity.Domain.ChannelDisplaySettings;
using WeightedTag = Identity.Domain.WeightedTag;

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

        public int GetPostsPerDay(long id)
        {
            var sql = @"with cte as 
(
    select @ChannelId as Id
    union all
    select t.ChildId as Id from cte 
        inner join [ChannelLink] t on cte.Id = t.Parentid
)
select case when DATEDIFF(dd, Min(Created), Max(Created)) > 0 then Count(*)/DATEDIFF(dd, Min(Created), Max(Created)) else 0 end as cnt
from ChannelItem ci
inner join cte on ci.ChannelId = cte.Id";

            return con.Connection.Query<int>(sql, new { ChannelId = id }, con).SingleOrDefault();
        }

        public int GetPopularity(long id)
        {
            return con.Connection.Query<int>("select count(*) from ChannelLink where ChildId = @Id", new { Id = id }, con).Single();
        }


        public IEnumerable<Channel> FindPublicChannelsByName(string name)
        {
            var encoded = "%" + name.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";
            return con.Connection.Query<Channel>("select * from Channel where IsPublic = 1 and Name like @Name", new { Name = encoded }, con);
        }

        public IEnumerable<Channel> All()
        {
            return con.Connection.Query<Channel>("select * from Channel", null, con);
        }


        public IEnumerable<Channel> AllPublic()
        {
            return con.Connection.Query<Channel>("select top 1000 * from Channel where IsPublic = 1", null, con);
        }

        public IEnumerable<Domain.ChannelScore> GetChannelScores(long channelId)
        {
            var sql = @"with cte as 
(
    select @ChannelId as Id
    union all
    select t.ChildId as Id from cte 
        inner join [ChannelLink] t on cte.Id = t.Parentid
)
select cs.ChannelId, c.Name as ChannelName, cs.Score
from ChannelScore cs
inner join cte on cs.ChannelId = cte.Id
inner join Channel c on c.Id = cs.ChannelId
where cs.ChannelId <> @ChannelId";
            return con.Connection.Query<Domain.ChannelScore>(sql, new {ChannelId = channelId}, con).ToList();
        }

        public IEnumerable<Domain.ChannelScore> GetTopChannelScores(int limit)
        {
            var sql = String.Format("select top {0} c.Id as ChannelId, c.Name as ChannelName, cs.Score from ChannelScore cs join Channel c on c.Id = cs.ChannelId order by Score desc", limit);
            return con.Connection.Query<Domain.ChannelScore>(sql, new { }, con).ToList();
        }

        public int UnreadCount(long userId, long channelId)
        {
            using (var pc = new PerfCounter("UnreadCount"))
            {
                return con.Connection.Query<int>(@"select COUNT(*) from ChannelItem
                                    left join ReadHistory on ChannelItem.PostId = ReadHistory.PostId and ReadHistory.UserId = @UserId
                                    left join ChannelLink cl on cl.ParentId = @ChannelId
                                    where (ChannelItem.ChannelId = cl.ChildId or ChannelItem.ChannelId = @ChannelId) and ReadHistory.UserId IS NULL",
                    new {UserId = userId, ChannelId = channelId}, con).Single();
            }
        }

        public void AddChannel(Channel channel)
        {
            channel.Id = con.Connection.Query<int>("insert Channel (Name, Created, IsPublic) values (@Name, @Created, @IsPublic); SELECT CAST(SCOPE_IDENTITY() as bigint)", channel, con).Single();
        }

        public void UpdateChannel(Channel channel, IEnumerable<long> subscriptions)
        {
            con.Connection.Execute("update Channel set Name=@Name, IsPublic=@IsPublic where Id=@Id", channel, con);

            con.Connection.Execute("delete from ChannelLink where ParentId = @ParentId ", new { ParentId = channel.Id }, con);

            foreach (var childId in subscriptions)
            {
                con.Connection.Execute("update ChannelLink set ChildId=@ChildId where ChildId=@ChildId and ParentId=@ParentId if @@rowcount = 0 insert ChannelLink values(@ParentId, @ChildId)", new { ParentId = channel.Id, ChildId = childId }, con);    
            }            
        }

        public IDictionary<long, int> PostCounts(DateTimeOffset since)
        {
            var sql = "select ci.ChannelId, count(*) as Cnt from ChannelItem ci join Channel c on c.Id = ci.ChannelId where ci.Created > @Since group by ci.ChannelId";
            return con.Connection
                .Query(sql, new {Since = since}, con)
                .Cast<IDictionary<string, object>>()
                .ToDictionary(row => (long)row["ChannelId"], row => (int)row["Cnt"]); 
        }

        public IDictionary<long, int> ReadCounts(DateTimeOffset since)
        {
            var sql = "select ci.ChannelId, count(*) as Cnt from ReadHistory his join ChannelItem ci on ci.PostId = his.PostId join Channel c on c.Id = ci.ChannelId where ci.Created > @Since group by ci.ChannelId";
            return con.Connection
                .Query(sql, new { Since = since }, con)
                .Cast<IDictionary<string, object>>()
                .ToDictionary(row => (long)row["ChannelId"], row => (int)row["Cnt"]);
        }

        public IEnumerable<Channel> GetAllDirectUpStreamChannels(long downStreamChannelId)
        {
            return con.Connection
                .Query<Channel>("select c.* from ChannelLink cl join Channel c on c.Id = cl.ChildId where cl.ParentId = @DownStreamChannelId", 
                new { DownStreamChannelId = downStreamChannelId }, con);
        }

        public void Delete(long userId, long channelId)
        {
            var cnt = con.Connection.Query<int>("select count(*) from ChannelOwner where UserId=@UserId and ChannelId=@ChannelId and IsLocked=0",
                new{ UserId = userId, ChannelId = channelId }, con).Single();

            if (cnt == 0)
            {
                throw new Exception("Access denied, you're not allowed to delete this channel");
            }
            
            con.Connection.Execute("delete from ChannelLink where ParentId=@Id or ChildId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from ChannelOwner where ChannelId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from ChannelItem where ChannelId=@Id", new { Id = channelId }, con);
            con.Connection.Execute("delete from Channel where Id=@Id", new { Id = channelId }, con);
        }

        public IEnumerable<Channel> GetSubscriptions(long channelId)
        {
            using (var pc = new PerfCounter("GetSubscriptions"))
            {
                return
                    con.Connection.Query<Channel>(
                        "select c.* from ChannelLink cl join Channel c on c.Id = cl.ChildId where cl.ParentId = @ChannelId",
                        new {ChannelId = channelId}, con).ToList();
            }
        }

        public void RemoveSubscription(long parentId, long childId)
        {
            con.Connection.Execute("delete from ChannelLink where ParentId = @ParentId and ChildId = @ChildId", new { ParentId = parentId, ChildId = childId }, con);
        }

        public void AddSubscription(long parentId, long childId)
        {
            con.Connection.Execute("update ChannelLink set ChildId=@ChildId where ChildId=@ChildId and ParentId=@ParentId if @@rowcount = 0 insert ChannelLink values(@ParentId, @ChildId)", new { ParentId = parentId, ChildId = childId }, con);
        }

        public Feed FeedById(long id)
        {
            return con.Connection.Query<Feed>("select * from RssFeeder where Id=@Id", new { Id = id }, con).SingleOrDefault();
        }

        public void MarkAllAsRead(long userId, long channelId)
        {
            const string postIds = @"select distinct @UserId, ci.PostId, @Timestamp
from Post 
join ChannelItem ci on ci.PostId = Post.Id 
join [User] u on u.Id = @UserId
left join ChannelLink cl on cl.ChildId = ci.ChannelId and cl.ParentId = @ChannelId
left join ChannelOwner co on co.ChannelId = ci.ChannelId and co.UserId = @UserId
join Channel c on c.Id = ci.ChannelId
left join ReadHistory h on h.UserId=@UserId and h.PostId = ci.PostId
where (ci.ChannelId=@ChannelId or cl.ParentId=@ChannelId) and (co.ChannelId is not null or c.IsPublic = 1) and h.UserId is null
";

            con.Connection.Execute(String.Format("insert into ReadHistory (UserId,PostId,Timestamp) ({0})", postIds), new
            {
                UserId = userId, 
                ChannelId = channelId, 
                Timestamp = DateTimeOffset.Now
            }, con);            
        }

        public Feed FeedByUrl(string url)
        {
            var rssFeeder = con.Connection.Query<Feed>("select * from RssFeeder where Url=@Url", new { Url = url }, con).SingleOrDefault();

            return rssFeeder;
        }

        public Feed GetFeedById(long id)
        {
            return con.Connection.Query<Feed>("select * from RssFeeder where Id = @Id", new { Id = id }, con).SingleOrDefault();
        }

        public Feed GetFeed(string url, FeedType type)
        {
            var feed = con.Connection.Query<Feed>("select * from RssFeeder where Url = @Url", new { Url = url }, con).SingleOrDefault();

            if (feed == null)
            {
                var feedChannel = new Channel
                {
                    Name = url,
                    IsPublic = true,
                    Created = DateTimeOffset.Now
                };
                AddChannel(feedChannel);
                feed = new Feed
                {
                    Url = url,
                    Type = type,
                    ChannelId = feedChannel.Id
                };

                feed.Id = con.Connection.Query<int>("insert RssFeeder (Url,[Type],ChannelId) values (@Url,@Type,@ChannelId); SELECT CAST(SCOPE_IDENTITY() as bigint)", feed, con).Single();
            }

            return feed;
        }

        public IEnumerable<Feed> AllFeeds()
        {
            return con.Connection.Query<Feed>("select RssFeeder.* from RssFeeder", null, con);
        }

        public IEnumerable<Feed> OutOfSyncFeeds(TimeSpan timeSpan)
        {
            return con.Connection.Query<Feed>("select RssFeeder.* from RssFeeder where LastFetch is null or DATEDIFF(mi, LastFetch, @Now) >= @TotalMinutes", new { DateTimeOffset.Now, timeSpan.TotalMinutes }, con);
        }

        public IEnumerable<WebScraper> OutOfSyncWebScrapers(TimeSpan timeSpan)
        {
            return con.Connection.Query<WebScraper>("select WebScraper.* from WebScraper where LastFetch is null or DATEDIFF(mi, LastFetch, @Now) >= @TotalMinutes", new { DateTimeOffset.Now, timeSpan.TotalMinutes }, con);
        }

        public void UpdateFeed(Feed feed)
        {
            con.Connection.Execute("update RssFeeder set LastFetch=@LastFetch, Url=@Url where Id=@Id", feed, con);
        }
        
        public IEnumerable<string> GetFeedTags(long rssFeederId)
        {
            return con.Connection.Query<string>("select Tag from FeederTags where RssFeederId=@RssFeederId", new { RssFeederId = rssFeederId }, con);
        }

        public void UpdateTagsOfFeed(long rssFeederId, IEnumerable<string> tags)
        {
            con.Connection.Execute("delete from FeederTags where RssFeederId=@RssFeederId", new { RssFeederId = rssFeederId }, con);
            foreach (var tag in tags)
            {
                con.Connection.Execute("insert FeederTags values(@RssFeederId,@Tag)", new { RssFeederId = rssFeederId, Tag = tag }, con);
            }
        }

        public IEnumerable<UnreadCount> GetUneadCounts(long userId)
        {
            var sql = @"select Channels.Id as ChannelId, Count(*) as Count from 
                        ( 
                            select co.ChannelId as Id from ChannelOwner co where co.UserId = @UserId
                            union select cl.ChildId as Id from ChannelLink cl join ChannelOwner co on cl.ParentId = co.ChannelId and co.UserId = @UserId
                        ) Channels
                        join ChannelItem ci on Channels.Id = ci.ChannelId                        
                        left join ReadHistory on ci.PostId = ReadHistory.PostId and ReadHistory.UserId = @UserId
                        where ReadHistory.UserId IS NULL
                        group by Channels.Id";

            return con.Connection.Query<UnreadCount>(sql, new { UserId = userId }, con);
        }

        public void UpdateChannelScores(IEnumerable<KeyValuePair<long, double>> scores)
        {
            con.Connection.Execute(@"delete from ChannelScore", new { }, con);

            var table = new DataTable();
            table.TableName = "ChannelScore";
            table.Columns.Add(new DataColumn("ChannelId", typeof(long)));
            table.Columns.Add(new DataColumn("Score", typeof(double)));

            var rows = scores.Select(x =>
            {
                var row = table.NewRow();
                row["Score"] = x.Value;
                row["ChannelId"] = x.Key;
                return row;
            });

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            BulkCopy.Copy((SqlConnection)con.Connection, table, (SqlTransaction)con);
        }

        public void UpdateChannelDisplaySettings(long userId, long channelId, ChannelDisplaySettings settings)
        {
            con.Connection.Execute("update ChannelDisplaySettings set Settings=@SerializedSettings where UserId=@UserId and ChannelId=@ChannelId if @@rowcount = 0 insert ChannelDisplaySettings values(@ChannelId, @UserId, @SerializedSettings)", new { UserId = userId, ChannelId = channelId, SerializedSettings = JsonConvert.SerializeObject(settings) }, con);
        }

        public ChannelDisplaySettings GetChannelDisplaySettings(long userId, long channelId)
        {
            var sql = "select Settings from ChannelDisplaySettings where UserId = @UserId and ChannelId = @ChannelId";

            var result = con.Connection
                .Query<string>(sql, new { UserId = userId, ChannelId = channelId }, con)
                .SingleOrDefault();

            if (result != null)
            {
                return JsonConvert.DeserializeObject<ChannelDisplaySettings>(result);
            }
            else
            {
                return ChannelDisplaySettings.New();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
