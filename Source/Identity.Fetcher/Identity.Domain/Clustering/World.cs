using System;
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

        public void Add(Item d)
        {
            if (!Clusters.Any())
            {
                Clusters.Add(new Cluster(d));
                return;
            }
            
            var distances = Clusters.Select(c => new { Cluster = c, Distance = VectorDistanceMeasure.Get(c.Centroid, d.WordVector) }).ToList();      
            var min = distances.Any() ? distances.MinBy(x => x.Distance) : null;

            Console.WriteLine("Min distance = " + (min != null ? min.Distance : double.NaN));

            if (min != null && min.Distance < threshold)
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
            var distances = Clusters.Where(c => c != cluster).Select(c => new { Cluster = c, Distance = VectorDistanceMeasure.Get(c.Centroid, cluster.Centroid) }).ToList();
            var min = distances.Any() ? distances.MinBy(x => x.Distance) : null;

            if (min != null && min.Distance < threshold)
            {
                Console.WriteLine("Merging cluster. min distance = " + (min != null ? min.Distance : double.NaN));
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