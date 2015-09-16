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
    public class PostRepository : IDisposable
    {
        private readonly IDbTransaction con;

        public PostRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public bool PostExists(string uri)
        {
            return con.Connection.Query<int>("select count(*) from Post where Uri=@Uri", new { Uri = uri }, con).Single() > 0;
        }

        public Post GetByUrl(string uri)
        {
            return con.Connection.Query<Post>("select * from Post where Uri=@Uri", new { Uri = uri }, con).SingleOrDefault();
        }

        public IEnumerable<Post> TopPosts(int count)
        {
            var sql = @"select * from Post where Id in 
  (select top {0} ci.PostId from ChannelItem ci
  where Created > @Timestamp and ci.UserId <> 2 and ci.UserId <> 5
  group by ci.PostId
  order by count(*) desc)";

            return con.Connection.Query<Post>(String.Format(sql, count), new { Timestamp = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)) }, con);
        }

        public void AddPost(Post post)
        {
            post.Id = con.Connection.Query<long>("insert Post values(@Created,@Title,@Description,@Uri); SELECT CAST(SCOPE_IDENTITY() as bigint)", post, con).Single();
        }

        public void UpdatePost(Post post)
        {
            con.Connection.Execute("update Post set Title=@Title, Description=@Description, Uri=@Uri where Id=@Id", post, con);
        }

        public void TagPost(long postId, IEnumerable<string> tags)
        {
            con.Connection.Execute("delete from Tagged where PostId=@PostId", new { PostId = postId }, con);
            foreach (var tag in tags)
            {
                con.Connection.Execute("insert Tagged values(@PostId,@Tag)", new { PostId = postId, Tag = tag }, con);    
            }
        }

        public IEnumerable<Post> FindByTag(string tag)
        {
            var encodedTag = "%" + tag.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";

            return con.Connection.Query<Post>(@"select Post.* from Post join Tagged t on t.PostId = Post.Id where t.Tag like @Tag", new { Tag = encodedTag }, con);
        }

        public IEnumerable<Post> PostsFromChannel(long userId, bool onlyUnread, long channelId, DateTimeOffset timestamp, int fromIndex, string orderBy)
        {
            if (orderBy == "Added")
            {
                orderBy = "XX.Added desc";
            }
            else if (orderBy == "Popularity")
            {
                orderBy = "XX.pop desc, pop.Popularity desc";
            }
            else
            {
                throw new Exception("Unexpected orderby clause: " + orderBy);
            }



            var postIds = @"select ci.PostId, count(*) as pop, min(ci.Created) as Added
from Post 
join ChannelItem ci on ci.PostId = Post.Id 
join [User] u on u.Id = @UserId
left join ChannelLink cl on cl.ChildId = ci.ChannelId and cl.ParentId = @ChannelId
where ci.ChannelId=@ChannelId or cl.ParentId=@ChannelId
group by ci.PostId";

            var postData = @"
select Post.*,
XX.pop as CPop,
XX.Added as Added,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred, 
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read],
CASE WHEN pop.Popularity IS NULL THEN 0 ELSE pop.Popularity END as Popularity,
ROW_NUMBER() OVER (ORDER BY {2}) AS RowNum
from Post 
join 
({0}) as XX on XX.PostId = Post.Id
join [User] u on u.Id = @UserId
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
left join Popularity pop on pop.PostId = Post.Id
where Post.Created < @Timestamp {1}";

            var paged = @"select * from ({0}) as TBL where TBL.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+30)";

            var sql = String.Format(paged, String.Format(postData, postIds, onlyUnread ? " and ReadHistory.Timestamp IS NULL" : "", orderBy));




//            var sql = @"select * from 
//(select Post.*, poster.Id as PosterId, poster.Username as PosterUserName,
//CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
//CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
//CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred, 
//CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read],
//dateadd(mi, datediff(mi, 0, ci.Created), 0) as Added, 
//CASE WHEN pop.Popularity IS NULL THEN 0 ELSE pop.Popularity END as Popularity,
//CASE WHEN userpop.Popularity IS NULL THEN 0 ELSE userpop.Popularity END as UserSpecificPopularity,
//ROW_NUMBER() OVER (ORDER BY {1}) AS RowNum
//from Post 
//join ChannelItem ci on ci.PostId = Post.Id 
//join [User] poster on poster.Id = ci.UserId 
//join [User] u on u.Id = @UserId 
//left join ChannelLink cl on cl.ChildId = ci.ChannelId and cl.ParentId = @ChannelId
//left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
//left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
//left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
//left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId 
//left join Popularity pop on pop.PostId = Post.Id
//left join UserSpecificPopularity userpop on userpop.PostId = Post.Id and userpop.UserId=@UserId
//where (ci.ChannelId=@ChannelId or cl.ParentId=@ChannelId)and Post.Created < @Timestamp {0}) as TBL
//where TBL.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+30)";

            //sql = String.Format(sql, onlyUnread ? " and ReadHistory.Timestamp IS NULL" : "", orderBy);

            return con.Connection.Query<Post>(sql, new { ChannelId = channelId, UserId = userId, Timestamp = timestamp, FromIndex = fromIndex}, con);                
        }

        public Post GetById(long id, long userId)
        {
            var sql = @"
select Post.*,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred, 
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read],
CASE WHEN userpop.Popularity IS NULL THEN 0 ELSE userpop.Popularity END as UserSpecificPopularity,
CASE WHEN pop.Popularity IS NULL THEN 0 ELSE pop.Popularity END as Popularity
from Post 
join [User] u on u.Id = @UserId 
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId 
left join Popularity pop on pop.PostId = Post.Id
left join UserSpecificPopularity userpop on userpop.PostId = Post.Id and userpop.UserId=@UserId
where Post.Id = @Id
";

            return con.Connection.Query<Post>(sql, new { Id = id, UserId = userId }, con).SingleOrDefault();
        }

        public IEnumerable<Tagged> Tags(long postId)
        {
            return con.Connection.Query<Tagged>("select * from Tagged where PostId=@PostId", new { PostId = postId }, con);
        }

        public IEnumerable<Channel> PublishedIn(long postId)
        {
            return con.Connection.Query<Channel>(@"
            select * from Channel where Id in (select top 5 Id from Channel 
            join ChannelItem ci on ci.ChannelId = Channel.Id and ci.PostId = @PostId
            left join Subscription s on s.ChannelId = Channel.Id
            group by Channel.Id
            order by COUNT(*))", new { PostId = postId }, con);
        }

        public IEnumerable<WeightedTag> TopTags(int count)
        {
            var sql = @"select top {0} count(*) as Weight, Tag as Text from Tagged 
                        join ChannelItem ci on ci.PostId = Tagged.PostId
                        where ci.Created > @Timestamp and ci.UserId <> 2 and ci.UserId <> 5
                        group by Tag
                        order by count(*) desc";

            return con.Connection.Query<WeightedTag>(String.Format(sql, count), new { Timestamp = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)) }, con);
        }

        public void Dispose()
        {
        }
    }
}
