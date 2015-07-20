using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class Comment
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long PostId { get; set; }

        public string Text { get; set; }

        public DateTime Created { get; set; }

        public int ReplyingTo { get; set; }

        public string Author { get; set; }
    }
}
