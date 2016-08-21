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

        public bool WebScraperItemAlreadyPosted(string title, DateTimeOffset created, long webScraperId)
        {
            return con.Connection.Query<Post>(@"select * from Post join WebScraperItem fi on fi.PostId = Post.Id 
                                                where Title=@Title and fi.Created=@Created and fi.WebScraperId=@WebScraperId", new
            {
                Title = title,
                Created = created,
                WebScraperId = webScraperId
            }, con).Any();
        }

        public void AddWebScraperItem(long webScraperId, long postId, DateTimeOffset created)
        {
            con.Connection.Execute("update WebScraperItem set PostId=@PostId where PostId=@PostId and WebScraperId=@WebScraperId if @@rowcount = 0 insert WebScraperItem values(@WebScraperId, @PostId, @Created)", new { WebScraperId = webScraperId, PostId = postId, Created = created }, con);
        }

        public bool FeedItemAlreadyPosted(string title, DateTimeOffset created, long rssFeederId)
        {
            return con.Connection.Query<Post>(@"select * from Post join FeedItem fi on fi.PostId = Post.Id 
                                                where Title=@Title and fi.Created=@Created and fi.RssFeederId=@RssFeederId", new
            {
                Title = title, 
                Created = created, 
                RssFeederId = rssFeederId
            }, con).Any();
        }

        public void AddFeedItem(long rssFeederId, long postId, DateTimeOffset created)
        {
            con.Connection.Execute("update FeedItem set PostId=@PostId where PostId=@PostId and RssFeederId=@RssFeederId if @@rowcount = 0 insert FeedItem values(@RssFeederId, @PostId, @Created)", new { RssFeederId = rssFeederId, PostId = postId, Created = created }, con);
        }

        public IEnumerable<Post> TopPosts(int count, long userId)
        {
            var sql = @"select * from Post where Id in 
  (select top {0} ci.PostId from ChannelItem ci
    join Channel c on c.Id = ci.ChannelId
    left join ChannelOwner co on co.ChannelId = ci.ChannelId and co.UserId = @UserId
  where (co.ChannelId is not null or c.IsPublic = 1) and ci.Created > @Timestamp and ci.UserId <> 2 and ci.UserId <> 5
  group by ci.PostId
  order by count(*) desc)";

            return con.Connection.Query<Post>(String.Format(sql, count), new { UserId = userId, Timestamp = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)) }, con);
        }

        public void AddPost(Post post, bool tokenizeTitle)
        {
            post.Id = con.Connection.Query<long>("insert Post values(@Created,@Title,@Description,@Uri); SELECT CAST(SCOPE_IDENTITY() as bigint)", post, con).Single();
            if (tokenizeTitle)
            {
                StoreTokenizedTitle(post);
            }            
        }

        public void UpdatePost(Post post)
        {
            con.Connection.Execute("update Post set Title=@Title, Description=@Description, Uri=@Uri where Id=@Id", post, con);
        }

        //TODO: optimize this!
        public void TagPost(long postId, IEnumerable<string> tags)
        {
            con.Connection.Execute("delete from Tagged where PostId=@PostId", new { PostId = postId }, con);
            foreach (var tag in tags)
            {
                con.Connection.Execute("update Tag set Name=@Tag where Name=@Tag if @@rowcount = 0 insert Tag (Name) values(@Tag)", new { Tag = tag }, con);
                var tagId = con.Connection.Query<long>("select Id from Tag where Name=@Tag", new { Tag = tag }, con).Single();
                con.Connection.Execute("insert Tagged (PostId,TagId) values(@PostId,@TagId)", new { PostId = postId, TagId = tagId }, con);    
            }
        }

        public void AutoTagPost(long postId, long tagId)
        {
            con.Connection.Execute("insert Tagged (PostId,TagId,Confirmed) values(@PostId,@TagId,false)", new { PostId = postId, TagId = tagId }, con);
        }

        //TODO: optimize this!
        private void StoreTokenizedTitle(Post post)
        {            
            con.Connection.Execute("delete from PostTitleWords where PostId=@PostId", new { PostId = post.Id }, con);
            foreach (var token in post.TokenizedTitle)
            {
                con.Connection.Execute("update Word set Contents=@Word where Contents=@Word if @@rowcount = 0 insert Word (Contents) values(@Word)", new { Word = token }, con);
                var wordId = con.Connection.Query<long>("select Id from Word where Contents=@Word", new { Word = token }, con).Single();
                con.Connection.Execute("update PostTitleWords set Count=Count+1 where PostId=@PostId and WordId=@WordId if @@rowcount = 0 insert PostTitleWords (PostId,WordId) values(@PostId,@WordId)", new { PostId = post.Id, WordId = wordId }, con);
            }
        }

        //TODO: add paging here
        public IEnumerable<Post> FindByTitleOrTag(string tag)
        {
            var encodedTag = tag.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";

            return con.Connection.Query<Post>(@"select top 100 Post.* from Post left join Tagged t on t.PostId = Post.Id join Tag on Tag.Id = Tagged.TagId where Tag.Name = @EncodedTag", new { EncodedTag = encodedTag, Tag = tag }, con);
        }

        //TODO: add paging here
        public IEnumerable<Post> ReadHistory(long userId)
        {
            return con.Connection.Query<Post>(@"select top 100 Post.* from Post 
                                                join ReadHistory h on h.PostId = Post.Id and h.UserId = @UserId 
                                                order by h.Timestamp desc", new { UserId = userId }, con);
        }

        class ChannelQueryResult
        {
            public int RowNum { get; set; }

            public long PostId { get; set; }

            public DateTimeOffset Added { get; set; }

            public int Popularity { get; set; }
        }

        public IEnumerable<Post> PostsFromChannel(long userId, bool onlyUnread, long channelId, DateTimeOffset timestamp, int fromIndex, string orderBy, int pageSize)
        {
            int orderByColumn;

            switch (orderBy)
            {
                case "Added":
                    orderByColumn = 1;
                    break;
                case "Popularity":
                    orderByColumn = 2;
                    break;
                default:
                    throw new Exception("Unrecognized sort column " + orderBy);
            }           

            var sp = onlyUnread ? "FetchUnreadPostIds" : "FetchPostIds";

            var channelQueryResult = con.Connection.Query<ChannelQueryResult>(sp, 
                new { ChannelId = channelId, UserId = userId, FromIndex = fromIndex, PageSize = pageSize, OrderByColumn = orderByColumn }, 
                con, true, null, CommandType.StoredProcedure).ToList();

            return GetByIds(channelQueryResult.Select(cqr => cqr.PostId), userId)
                .Join(channelQueryResult, x => x.Id, x => x.PostId, (p,cqr) => new {p,cqr})
                .OrderBy(x => x.cqr.RowNum)
                .Select(x => x.p)
                .ToList();
        }

        private IEnumerable<Post> GetByIds(IEnumerable<long> ids, long userId)
        {
            var idTable = new DataTable();
            idTable.Columns.Add("id", typeof(long));
            foreach (var id in ids)
            {
                idTable.Rows.Add(id);
            }

            return con.Connection.Query<Post>("FetchPostsFromIds",
                new { PostIds = idTable, UserId = userId }, con, true, null, CommandType.StoredProcedure);                            
        }

        public Post GetById(long id, long userId)
        {
            return GetByIds(new[] {id}, userId).SingleOrDefault();
        }

        public IEnumerable<Tagged> Tags(long postId)
        {
            using (var pc = new PerfCounter("Tags"))
            {
                return con.Connection.Query<Tagged>("select Tagged.PostId, Tag.Name as Tag from Tagged join Tag on Tag.Id = Tagged.TagId where PostId=@PostId", new {PostId = postId},
                    con);
            }
        }

        public IEnumerable<Tagged> Tags(IEnumerable<long> postIds)
        {
            using (var pc = new PerfCounter("Tags"))
            {
                return con.Connection.Query<Tagged>("select Tagged.PostId, Tag.Name as Tag from Tagged join Tag on Tag.Id = Tagged.TagId where Tagged.PostId in @PostIds", new { PostIds = postIds },
                    con);
            }
        }

        public IEnumerable<PublishedIn> PublishedIn(IEnumerable<long> postIds, long userId)
        {
            using (var pc = new PerfCounter("PublishedIn"))
            {
                return con.Connection.Query<PublishedIn>(@"
            select Channel.Id as ChannelId, Channel.Name as ChannelName, ci.PostId, COUNT(*) as Count from Channel 
            join ChannelItem ci on ci.ChannelId = Channel.Id and ci.PostId in @PostIds
            left join ChannelOwner co on co.ChannelId = Channel.Id and co.UserId = @UserId
            left join Subscription s on s.ChannelId = Channel.Id
            where co.ChannelId is not null or Channel.IsPublic = 1
            group by Channel.Id, Channel.Name, ci.PostId
            order by COUNT(*)", new {PostIds = postIds, UserId = userId}, con);
            }
        }

        public IEnumerable<WeightedTag> TopTags(int count)
        {
            var sql = @"select top {0} count(*) as Weight, Tag.Name as Text from Tagged 
                        join Tag on Tag.Id = Tagged.TagId
                        join ChannelItem ci on ci.PostId = Tagged.PostId
                        where ci.Created > @Timestamp and ci.UserId <> 2 and ci.UserId <> 5
                        group by Tag.Name
                        order by count(*) desc";

            return con.Connection.Query<WeightedTag>(String.Format(sql, count), new { Timestamp = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(7)) }, con);
        }

        public void Dispose()
        {
        }
    }
}
