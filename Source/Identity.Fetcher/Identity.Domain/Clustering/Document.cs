using System;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Clustering
{
    public class Document
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Added { get; set; }
        public int[] WordVector { get; set; }
    }
}
