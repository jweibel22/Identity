using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Identity.Domain.Clustering
{
    public class World
    {
        public IList<Cluster> Clusters { get; set; }

        private readonly double threshold;

        public World(double threshold)
        {
            this.threshold = threshold;
            Clusters = new List<Cluster>();
        }

        public World(double threshold, List<Cluster> clusters)
        {
            this.threshold = threshold;
            Clusters = clusters;
        }

        public void Add(Document d)
        {
            if (!Clusters.Any())
            {
                Clusters.Add(new Cluster(d));
                return;
            }
            
            var distances = Clusters.Select(c => new { Cluster = c, Distance = DistanceMeasure.Get(c.Centroid, d.WordVector) });
            var min = distances.MinBy(x => x.Distance);

            if (min.Distance < threshold)
            {
                min.Cluster.Add(d);
                Merge(min.Cluster);
            }
            else
            {
                Clusters.Add(new Cluster(d));
            }
        }

        private void Merge(Cluster cluster)
        {
            var distances = Clusters.Where(c => c != cluster).Select(c => new { Cluster = c, Distance = DistanceMeasure.Get(c.Centroid, cluster.Centroid) });
            var min = distances.MinBy(x => x.Distance);

            if (min.Distance < threshold)
            {
                //			if (cluster.Documents.First().Id == 361899 || min.Cluster.Documents.First().Id == 361899)
                //			{
                //				String.Format("Merging {0} into {1}. Get={2}", min.Cluster.FriendlyName, cluster.FriendlyName, min.Get).Dump();
                //			}

                cluster.Merge(min.Cluster);
                Clusters.Remove(min.Cluster);
                Merge(cluster);

            }
        }
    }
}