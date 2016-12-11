using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Identity.Infrastructure.Repositories
{
    public class OntologyRepository
    {
        private readonly IDbTransaction con;

        public OntologyRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public IList<Domain.Clustering.Document> GetNextClusteringWindow(long ontologyId)
        {
            var windowStarts = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var sql = @"select Post.Id, Title, Description, Added from Post
join
(select p.Id, Min(ci.Created) as Added
from ChannelItem ci join Post p on ci.PostId = p.Id
join OntologyMembers om on om.ChannelId = ci.ChannelId
where om.OntologyId = @OntologyId and ci.Created > @From
group by p.Id) as X on X.Id = Post.Id
";

            return con.Connection.Query<Domain.Clustering.Document>(sql, new { From = windowStarts, OntologyId = ontologyId }, con).ToList();
        }

        public void UpdateClusters(long ontologyId, IList<Domain.Clustering.Cluster> clusters)
        {
            con.Connection.Execute("delete from PostCluster where OntologyId=@OntologyId", new { OntologyId = ontologyId }, con);
            con.Connection.Execute("delete from PostClusterMember where OntologyId=@OntologyId", new { OntologyId = ontologyId }, con);

            var clusterValues = new List<dynamic>();
            var values = new List<dynamic>();

            for (var idx = 0; idx < clusters.Count; idx++)
            {
                values.AddRange(clusters[idx].Documents.Select(d => new {OntologyId = ontologyId, PostId = d.Id, ClusterId = idx + 1}));
                clusterValues.Add(new { OntologyId = 1, ClusterId = idx+1, LatestAdded = clusters[idx].Documents.Max(d => d.Added) });
            }

            con.Connection.Execute("insert into PostCluster (OntologyId,ClusterId,LatestAdded) values (@OntologyId,@ClusterId,@LatestAdded)", clusterValues, con);
            con.Connection.Execute("insert into PostClusterMember (OntologyId,ClusterId,PostId) values (@OntologyId,@ClusterId,@PostId)", values, con);
        }
    }
}
