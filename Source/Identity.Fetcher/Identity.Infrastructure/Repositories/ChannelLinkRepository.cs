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
    public class ChannelIsDirtyEvent
    {
        public long Id { get; set; }

        public long ChannelId { get; set; }

        public DateTimeOffset Created { get; set; }
    }

    public class ChannelLinkRepository
    {
        private readonly IDbTransaction con;

        public ChannelLinkRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public ChannelLinkGraph GetGraph()
        {
            var userNodes = con.Connection.Query<long>("select id from [User]", new { }, con).ToList().Select(ChannelLinkNode.NewUserNode).ToList();
            var channelNodes = con.Connection.Query<long>("select id from Channel", new { }, con).ToList().Select(ChannelLinkNode.NewChannelNode).ToList();

            var ownerEdges =
                con.Connection.Query("select ChannelId, UserId from ChannelOwner", new { }, con)
                    .ToList()
                    .Join(userNodes, x => x.UserId, x => x.Id, (u, n) => new { u.ChannelId, To = n})
                    .Join(channelNodes, x => x.ChannelId, x => x.Id, (u, n) => new ChannelLinkEdge { From = n, To = u.To });

            var channelEdges =
                con.Connection.Query("select ParentId, ChildId from ChannelLink", new { }, con)
                    .ToList()
                    .Join(channelNodes, x=> x.ParentId, x => x.Id, (u, n) => new { u.ChildId, To = n })
                    .Join(channelNodes, x => x.ChildId, x => x.Id, (u, n) => new ChannelLinkEdge { From = n, To = u.To });

            return new ChannelLinkGraph(userNodes.Union(channelNodes), ownerEdges.Union(channelEdges));
        }

        public void UpdateUnreadCounts(long userId, long channelId)
        {
            var sql = @"Update ChannelOwner set UnreadCount = (SELECT Cnt FROM [dbo].[ftUnreadPosts] (@ChannelId,@UserId)) where ChannelId = @ChannelId and UserId = @UserId";
            con.Connection.Execute(sql, new { UserId = userId, ChannelId = channelId }, con);
        }

        public bool CyclesExist(long channelId)
        {
            try
            {
                var sql = @"with cte as 
(
    select @ChannelId as Id
    union all
    select t.ChildId as Id from cte 
        inner join [ChannelLink] t on cte.Id = t.Parentid
)
select Channel.Id, Name from Channel
inner join cte on Channel.Id = cte.Id";

                con.Connection.Execute(sql, new {ChannelId = channelId}, con);

                return false;
            }
            catch (SqlException e)
            {
                if (e.Number == 530)
                {
                    return true;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
