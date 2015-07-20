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
        private readonly IDbConnection con;

        public UserRepository(IDbConnection con)
        {
            this.con = con;
        }

        public User FindByName(string username)
        {
            return con.Query<User>("select * from [User] where Username=@Username", new { Username = username }).SingleOrDefault();
        }

        public void Publish(long userId, long channelId, long postId)
        {
            con.Execute("update ChannelItem set PostId=@PostId where PostId=@PostId and ChannelId=@ChannelId if @@rowcount = 0 insert ChannelItem values(@ChannelId, @PostId, @UserId, @Created)", new { UserId = userId, ChannelId = channelId, PostId = postId, Created = DateTime.Now });
        }

        public void Remove(long userId, long channelId, long postId)
        {
            con.Execute("delete from ChannelItem where UserId=@UserId and ChannelId=@ChannelId and PostId=@PostId", new { UserId = userId, ChannelId = channelId, PostId = postId });
        }

        public void Owns(long userId, long channelId)
        {
            con.Execute("insert ChannelOwner values(@ChannelId, @UserId)", new { UserId = userId, ChannelId = channelId });
        }

        public void Read(long userId, long postId)
        {
            con.Execute("update ReadHistory set Timestamp=@Timestamp where UserId=@UserId and PostId=@PostId if @@rowcount = 0 insert ReadHistory values(@UserId, @PostId, @Timestamp)", new { UserId = userId, PostId = postId, Timestamp = DateTimeOffset.Now });
        }

        public bool IsRead(long userId, long postId)
        {
            return con.Query<long>("select count(*) from ReadHistory where UserId=@UserId and PostId=@PostId", new { UserId = userId, PostId = postId }).Single() > 0;
        }

        public void Subscribe(long userId, long channelId)
        {
            con.Execute("insert Subscription values(@ChannelId, @UserId)", new { UserId = userId, ChannelId = channelId });
        }

        public void Unsubscribe(long userId, long channelId)
        {
            con.Execute("delete from Subscription where UserId=@UserId and ChannelId=@ChannelId", new { UserId = userId, ChannelId = channelId });
        }

        public IEnumerable<Channel> Follows(long userId)
        {
            return con.Query<Channel>("select * from Channel c join Subscription s on c.Id = s.ChannelId where s.UserId = @UserId", new { UserId = userId });
        }

        public IEnumerable<Channel> Owns(long userId)
        {
            return con.Query<Channel>("select * from Channel c join ChannelOwner s on c.Id = s.ChannelId where s.UserId = @UserId", new { UserId = userId });
        }        

        public void Dispose()
        {
            
        }

    }
}
