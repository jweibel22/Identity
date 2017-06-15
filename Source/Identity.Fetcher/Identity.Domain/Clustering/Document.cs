using System;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Identity.Domain.Clustering
{
    public class Item
    {
        public long Id { get; set; }

        public Vector<double> WordVector { get; set; }

        public DateTimeOffset Added { get; set; }
    }

    public class Document
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Added { get; set; }
        public Vector<double> WordVector { get; set; }
    }
}
