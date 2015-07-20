using System;

namespace Identity.Infrastructure.DTO
{
    public class Comment
    {
        public string Body { get; set; }

        public int Upvotes { get; set; }

        public string Author { get; set; }

        public DateTime Created { get; set; }

        public long UserId { get; set; }

        public int CreatedHoursAgo { get { return (int)DateTime.Now.Subtract(Created).TotalHours; }}
    }
}