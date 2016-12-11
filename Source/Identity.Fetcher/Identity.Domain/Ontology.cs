using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class Ontology
    {
        public long Id { get; set; }

        public DateTimeOffset Updated { get; set; }
    }

    public class PostClusterMember
    {
        public long OntologyId { get; set; }

        public long PostId { get; set; }

        public long ClusterId { get; set; }
    }    
}
