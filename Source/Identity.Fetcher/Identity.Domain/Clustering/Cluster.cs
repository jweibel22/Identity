using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Domain.Clustering
{

    public class Cluster
    {
        public double[] Centroid { get; set; }

        public double[] Sums { get; set; }

        public List<Document> Documents { get; set; }

        private readonly int n;

        public Cluster(List<Document> documents)
        {
            this.Documents = documents;
            this.n = documents.First().WordVector.Length;
            Sums = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                Sums[i] += documents.Sum(x => x.WordVector[i]);
            }

            ComputeCentroid();
        }

        public Cluster(Document d)
        {
            Documents = new List<Document>();
            n = d.WordVector.Length;
            Sums = new double[n];
            Add(d);
        }

        public void Add(Document d)
        {
            //		if (d.Id == 362179)
            //		{
            //			String.Format("Adding Id={0}, Title={1}", d.Id, d.Title).Dump();
            //		}
            //		else if (Documents.Any() && Documents.First().Id == 362179)
            //		{
            //			var v = d.WordVector.Select(x => (double)x).ToArray();
            //			String.Format("Adding Id={0}, Title={3}. CosineSim={1},Get={2}", d.Id, CosineSim(Centroid, v), Get(Centroid, v), d.Title).Dump();
            //		}

            if (d.WordVector.Length != n)
            {
                throw new ApplicationException("Expected a vector with " + n + " elements");
            }
            Documents.Add(d);

            for (int i = 0; i < n; i++)
            {
                Sums[i] += d.WordVector[i];
            }
            ComputeCentroid();
        }

        public void Merge(Cluster c)
        {
            //String.Format("Merging {0} into {1}", c.FriendlyName, FriendlyName).Dump();
            Documents.AddRange(c.Documents);

            for (int i = 0; i < n; i++)
            {
                Sums[i] += c.Documents.Sum(x => x.WordVector[i]);
            }

            ComputeCentroid();
        }

        private void ComputeCentroid()
        {
            Centroid = Enumerable.Range(0, n).Select(i => Sums[i] / Documents.Count).ToArray();
        }

        public string FriendlyName
        {
            get { return Documents.First().Title; }
        }
    }
}