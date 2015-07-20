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
        private readonly IDbConnection con;

        public PostRepository(IDbConnection con)
        {
            this.con = con;
        }

        public bool PostExists(string uri)
        {
            return con.Query<int>("select count(*) from Post where Uri=@Uri", new { Uri = uri }).Single() > 0;
        }

        public void AddPost(Post post)
        {
            post.Id = con.Query<long>("insert Post values(@Created,@Title,@Description,@Uri); SELECT CAST(SCOPE_IDENTITY() as bigint)", post).Single();
        }

        public void UpdatePost(Post post)
        {
            con.Execute("update Post set Title=@Title, Description=@Description, Uri=@Uri where Id=@Id", post);
        }

        public void TagPost(long postId, IEnumerable<string> tags)
        {
            con.Execute("delete from Tagged where PostId=@PostId", new { PostId = postId });
            foreach (var tag in tags)
            {
                con.Execute("insert Tagged values(@PostId,@Tag)", new { PostId = postId, Tag = tag });    
            }
        }

        public IEnumerable<Post> FindByTag(string tag)
        {
            var encodedTag = "%" + tag.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]") + "%";

            return con.Query<Post>(@"select Post.* from Post join Tagged t on t.PostId = Post.Id where t.Tag like @Tag", new { Tag = encodedTag });
        }

        public IEnumerable<Post> UnreadPostsFromChannel(long userId, bool onlyUnread, long channelId)
        {
            return con.Query<Post>(@"select Post.*, ci.Created as Added from Post 
                                        left join ReadHistory h on h.PostId = Post.Id and h.UserId = @UserId
                                        join ChannelItem ci on ci.PostId = Post.Id where ci.ChannelId=@ChannelId and h.PostId IS NULL",
                    new { ChannelId = channelId, UserId = userId });                            
        }

        public IEnumerable<Post> PostsFromChannel(long channelId)
        {
            return con.Query<Post>(@"select Post.*, ci.Created as Added from Post 
                                        join ChannelItem ci on ci.PostId = Post.Id where ci.ChannelId=@ChannelId",
                   new { ChannelId = channelId });                
        }

        public Post GetById(long id)
        {
            return con.Query<Post>("select * from Post where Id=@Id", new { Id = id }).SingleOrDefault();
        }

        public IEnumerable<Tagged> Tags(long postId)
        {
            return con.Query<Tagged>("select * from Tagged where PostId=@PostId", new { PostId = postId });
        }
        

        public void Dispose()
        {
        }
    }
}
