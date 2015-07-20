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
    public class CommentRepostitory : IDisposable
    {
        private readonly IDbConnection con;

        public CommentRepostitory(IDbConnection con)
        {
            this.con = con;
        }

        public void AddComment(Comment comment)
        {
            comment.Id = con.Execute("insert Comment values(@UserId, @PostId, @Text, @Created, @ReplyingTo); SELECT CAST(SCOPE_IDENTITY() as bigint)", 
                new
                {
                    comment.UserId, comment.PostId, comment.Text, comment.Created, comment.ReplyingTo
                });
        }

        public IEnumerable<Comment> CommentsForPost(long postId)
        {
            return con.Query<Comment>("select Comment.*, [User].Username as Author from Comment join [User] on [User].Id = Comment.UserId where PostId=@PostId", new {PostId = postId});
        }
                
        public int CommentCount(long postId)
        {
            return con.Query<int>("select count(*) from Comment where PostId=@PostId", new { PostId = postId }).Single();
        }

        public IEnumerable<CommentCount> CommentCounts(long channelId)
        {
            return con.Query<CommentCount>(@"select Comment.PostId as Id, count(*) as [Count] from Comment join ChannelItem on ChannelItem.PostId = Comment.PostId 
                                    where ChannelItem.ChannelId=@ChannelId group by Comment.PostId", new { ChannelId = channelId });
        }

        public void Dispose()
        {
        }
    }

    public class CommentCount
    {
        public long Id { get; set; }

        public int Count { get; set; }
    }
}
