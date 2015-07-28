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
    public class UserRepository : IDisposable
    {
        private readonly IDbTransaction con;

        public UserRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public User FindByName(string username)
        {
            return con.Connection.Query<User>("select * from [User] where Username=@Username", new { Username = username }, con).SingleOrDefault();
        }

        public void Publish(long userId, long channelId, long postId)
        {
            con.Connection.Execute("update ChannelItem set PostId=@PostId where PostId=@PostId and ChannelId=@ChannelId if @@rowcount = 0 insert ChannelItem values(@ChannelId, @PostId, @UserId, @Created)", new { UserId = userId, ChannelId = channelId, PostId = postId, Created = DateTime.Now }, con);
        }

        public void Remove(long userId, long channelId, long postId)
        {
            con.Connection.Execute("delete from ChannelItem where UserId=@UserId and ChannelId=@ChannelId and PostId=@PostId", new { UserId = userId, ChannelId = channelId, PostId = postId }, con);
        }

        public void Owns(long userId, long channelId)
        {
            con.Connection.Execute("insert ChannelOwner values(@ChannelId, @UserId)", new { UserId = userId, ChannelId = channelId }, con);
        }

        public void Read(long userId, long postId)
        {
            con.Connection.Execute("update ReadHistory set Timestamp=@Timestamp where UserId=@UserId and PostId=@PostId if @@rowcount = 0 insert ReadHistory values(@UserId, @PostId, @Timestamp)", new { UserId = userId, PostId = postId, Timestamp = DateTimeOffset.Now }, con);
        }

        public bool IsRead(long userId, long postId)
        {
            return con.Connection.Query<long>("select count(*) from ReadHistory where UserId=@UserId and PostId=@PostId", new { UserId = userId, PostId = postId }, con).Single() > 0;
        }

        public void Subscribe(long userId, long channelId)
        {
            con.Connection.Execute("insert Subscription values(@ChannelId, @UserId)", new { UserId = userId, ChannelId = channelId }, con);
        }

        public void Unsubscribe(long userId, long channelId)
        {
            con.Connection.Execute("delete from Subscription where UserId=@UserId and ChannelId=@ChannelId", new { UserId = userId, ChannelId = channelId }, con);
        }

        public IEnumerable<Channel> Follows(long userId)
        {
            return con.Connection.Query<Channel>("select * from Channel c join Subscription s on c.Id = s.ChannelId where s.UserId = @UserId", new { UserId = userId }, con);
        }

        public IEnumerable<Channel> Owns(long userId)
        {
            return con.Connection.Query<Channel>("select * from Channel c join ChannelOwner s on c.Id = s.ChannelId where s.UserId = @UserId", new { UserId = userId }, con);
        }

        public IEnumerable<WeightedTag> GetTagCloud(long userId)
        {
            return con.Connection.Query<WeightedTag>(@"select top 20 count(*) as Weight, Tag as Text from Tagged join ChannelItem ci on ci.PostId = Tagged.PostId and ci.UserId = @UserId
                                            group by Tag order by COUNT(*) desc", new { UserId = userId }, con);
        }

        public IEnumerable<Post> GetFeed(long userId, DateTime timestamp, int fromIndex, string orderBy)
        {
            if (orderBy == "Added")
            {
                orderBy = "ci.Created";
            }

            var sql = @"
select * from (
select Post.*, 
CASE WHEN pop.Popularity IS NULL THEN 0 ELSE pop.Popularity END as Popularity,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred, 
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read],
dateadd(mi, datediff(mi, 0, ci.Created), 0) as Added, 
ROW_NUMBER() OVER (ORDER BY {0} desc) AS RowNum
from Post 
join ChannelItem ci on ci.PostId = Post.Id
join [User] u on u.Id = @UserId 
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
join Subscription s on s.ChannelId = ci.ChannelId and s.UserId = @UserId
left join Popularity pop on pop.PostId = Post.Id
left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId where ReadHistory.Timestamp is null
and Post.Created < @Timestamp) as TBL
where TBL.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+10)";

            sql = String.Format(sql, orderBy);

            return con.Connection.Query<Post>(sql, new { UserId = userId, FromIndex = fromIndex, Timestamp = timestamp }, con);
        }

        public void Dispose()
        {
            
        }

    }
}
