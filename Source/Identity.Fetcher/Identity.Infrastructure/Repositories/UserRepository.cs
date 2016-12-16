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

        public void AddUser(User user)
        {
            user.Id = con.Connection.Query<int>("insert [User] values (@Username, @SavedChannel, @StarredChannel, @LikedChannel, @IdentityId, @Inbox, @SubscriptionChannel); SELECT CAST(SCOPE_IDENTITY() as bigint)", user, con).Single();
        }

        public void AddLogin(User user, string loginProvider, string providerKey)
        {
            con.Connection.Execute("insert UserLogins values (@UserId, @ProviderKey, @LoginProvider)", new
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            }, con);
        }

        //TODO: add hashed password to user model
        public User FindUser(string userName, string password)
        {
            return con.Connection.Query<User>("select * from [User] where username = @Username",
                new
                {
                    Username = userName,
                    Password = password
                }, con).SingleOrDefault();
        }

        public Client FindClient(string clientId)
        {
            if (clientId == "ngAuthApp")
            {
                return new Client
                {
                    Active = true,
                    AllowedOrigin = "http://localhost:8010",
                    ApplicationType = ApplicationTypes.JavaScript,
                    Secret = "5YV7M1r981yoGhELyB84aC+KiYksxZf1OY3++C1CtRM=",
                    Id = "ngAuthApp",
                    RefreshTokenLifeTime = 7200,
                    Name = ""
                };
            }
            else
            {
                return null;
            }
        }

        public User Find(string loginProvider, string providerKey)
        {
            return con.Connection.Query<User>(
                "select * from [User] join UserLogins on [User].Id = UserLogins.UserId where UserLogins.ProviderKey = @ProviderKey AND UserLogins.LoginProvider = @LoginProvider",
                new { LoginProvider = loginProvider, ProviderKey = providerKey }, con)
                .SingleOrDefault();
        }

        public User GetById(int userId)
        {
            return con.Connection.Query<User>("select * from [User] where Id=@Id", new { Id = userId }, con).SingleOrDefault();
        }

        public User TryFindByName(string username)
        {
            return con.Connection.Query<User>("select * from [User] where Username=@Username", new { Username = username }, con).SingleOrDefault();
        }

        public User FindByName(string username)
        {
            var user = TryFindByName(username);

            //if (user == null)
            //{
            //    throw new Exception("No user with username " + username + " exist");
            //}

            return user;
        }

        public IEnumerable<User> SearchByName(string username)
        {
            return con.Connection.Query<User>("select * from [User] where Username like @Username", new { Username = username }, con);
        }

        public void Publish(long userId, long channelId, long postId)
        {
            con.Connection.Execute("update ChannelItem set PostId=@PostId where PostId=@PostId and ChannelId=@ChannelId if @@rowcount = 0 insert ChannelItem values(@ChannelId, @PostId, @UserId, @Created)", 
                new { UserId = userId, ChannelId = channelId, PostId = postId, Created = DateTimeOffset.Now }, con);
        }

        public void Remove(long userId, long channelId, long postId)
        {
            con.Connection.Execute("delete from ChannelItem where UserId=@UserId and ChannelId=@ChannelId and PostId=@PostId", new { UserId = userId, ChannelId = channelId, PostId = postId }, con);
        }

        public void Owns(long userId, long channelId, bool locked)
        {
            con.Connection.Execute("insert ChannelOwner (ChannelId, UserId, IsLocked) values(@ChannelId, @UserId, @IsLocked)", new { UserId = userId, ChannelId = channelId, IsLocked = locked }, con);
        }

        public int Read(long userId, long postId)
        {
            var sql = @"
update ReadHistory set Timestamp=@Timestamp where UserId=@UserId and PostId=@PostId 
if @@rowcount = 0 
	begin
		insert ReadHistory values(@UserId, @PostId, @Timestamp);
		select 1;
	end
else
	select 0;
";
            return con.Connection.Query<int>(sql, new { UserId = userId, PostId = postId, Timestamp = DateTimeOffset.Now }, con).Single();
        }

        public bool IsRead(long userId, long postId)
        {
            return con.Connection.Query<long>("select count(*) from ReadHistory where UserId=@UserId and PostId=@PostId", new { UserId = userId, PostId = postId }, con).Single() > 0;
        }

        public void Leave(long userId, long channelId)
        {
            var cnt = con.Connection.Query<int>("select count(*) from ChannelOwner where UserId=@UserId and ChannelId=@ChannelId and IsLocked=0",
                                                new { UserId = userId, ChannelId = channelId }, con).Single();

            if (cnt == 0)
            {
                throw new Exception("Access denied, you're not allowed to delete this channel");
            }

            con.Connection.Execute("delete from ChannelOwner where UserId=@UserId and ChannelId=@ChannelId", new { UserId = userId, ChannelId = channelId }, con);
        }

        public IEnumerable<OwnChannel> Owns(long userId)
        {
            return con.Connection.Query<OwnChannel>("select c.*, s.IsLocked, s.UnreadCount from Channel c join ChannelOwner s on c.Id = s.ChannelId where s.UserId = @UserId", new { UserId = userId }, con);
        }

        public IEnumerable<ChannelLink> ChannelLinks(long userId)
        {
            return con.Connection.Query<ChannelLink>("select co.ChannelId as DownStreamChannelId, c.Id as UpStreamChannelId from ChannelLink cl join ChannelOwner co on co.UserId = @UserId and co.ChannelId = cl.ParentId join Channel c on c.Id = cl.ChildId", new { UserId = userId }, con);
        }

        public IEnumerable<WeightedTag> GetTagCloud(long userId, long forUserId)
        {
            return con.Connection.Query<WeightedTag>(@"
                        select top 20 count(*) as Weight, Tag.Name as Text from Tagged 
                        join Tag on Tag.Id = Tagged.TagId
                        join ChannelItem ci on ci.PostId = Tagged.PostId and ci.UserId = @UserId                         
                        join Channel c on c.Id = ci.ChannelId
                        left join ChannelOwner co on co.ChannelId = c.Id and co.UserId = @ForUserId
                        where co.ChannelId is not null or c.IsPublic = 1                                            
                        group by Tag.Name order by COUNT(*) desc", new { UserId = userId, ForUserId = forUserId }, con);
        }

        public void BlockTag(long userId, string tag)
        {
            var tagIds = con.Connection.Query<long>("select Id from tag where Name = @Tag", new { Tag = tag }, con).ToList();
            if (tagIds.Any())
            {
                con.Connection.Execute(
                    "update BlockedTag set UserId=@UserId where UserId=@UserId and TagId=@TagId if @@rowcount = 0 insert BlockedTag values(@UserId, @TagId)",
                    new {UserId = userId, TagId = tagIds.First() }, con);
            }
        }

        public IEnumerable<long> BlockedTagIds(long userId)
        {
            return con.Connection.Query<long>("select TagId from BlockedTag where UserId = @UserId", new { UserId = userId }, con);
        }

        public void Dispose()
        {
            
        }

        public void Grant(long userId, long channelId)
        {
            con.Connection.Execute("update ChannelOwner set UserId=@UserId, ChannelId=@ChannelId where UserId=@UserId and ChannelId=@ChannelId if @@rowcount = 0 insert ChannelOwner values(@ChannelId, @UserId, @IsLocked)", new { UserId = userId, ChannelId = channelId, IsLocked = false }, con);
        }

        public void DecrementUnreadCount(long userId, long channelId, int unreadCount)
        {
            con.Connection.Execute("update ChannelOwner set UnreadCount = UnreadCount - @UnreadCount where UserId=@UserId and ChannelId=@ChannelId", new { UserId = userId, ChannelId = channelId, UnreadCount = unreadCount }, con);
        }
    }
}
