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
        private readonly IDbTransaction con;

        public CommentRepostitory(IDbTransaction con)
        {
            this.con = con;
        }

        public void AddComment(Comment comment)
        {
            comment.Id = con.Connection.Execute("insert Comment values(@UserId, @PostId, @Text, @Created, @ReplyingTo); SELECT CAST(SCOPE_IDENTITY() as bigint)", 
                new
                {
                    comment.UserId, comment.PostId, comment.Text, comment.Created, comment.ReplyingTo
                }, con);
        }

        public IEnumerable<Comment> CommentsForPost(long postId)
        {
            return con.Connection.Query<Comment>("select Comment.*, [User].Username as Author from Comment join [User] on [User].Id = Comment.UserId where PostId=@PostId", new { PostId = postId }, con);
        }
                
        public int CommentCount(long postId)
        {
            return con.Connection.Query<int>("select count(*) from Comment where PostId=@PostId", new { PostId = postId }, con).Single();
        }

        public class PostIdAndCount
        {
            public long PostId { get; set; }

            public int Count { get; set; }
        }

        public IEnumerable<PostIdAndCount> CommentCount(IEnumerable<long> postIds)
        {
            return con.Connection.Query<PostIdAndCount>("select PostId, count(*) as Count from Comment group by PostId having PostId in @PostIds", new { PostIds = postIds }, con);
        }

        public IEnumerable<CommentCount> CommentCounts(long channelId)
        {
            return con.Connection.Query<CommentCount>(@"select Comment.PostId as Id, count(*) as [Count] from Comment join ChannelItem on ChannelItem.PostId = Comment.PostId 
                                    where ChannelItem.ChannelId=@ChannelId group by Comment.PostId", new { ChannelId = channelId }, con);
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
