using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class OwnChannel : Channel
    {
        public int UnreadCount { get; set; }
    }

    public class ChannelLink
    {
        public long UpStreamChannelId { get; set; }

        public long DownStreamChannelId { get; set; }
    }

    public class Channel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public bool IsPublic { get; set; }

        public bool IsLocked { get; set; }
        

        public Channel()
        {
        }

        private Channel(string name)
        {
            Created = DateTimeOffset.Now;
            Name = name;
        }

        public static Channel New(string name)
        {
            return new Channel
            {
                Name = name,
                Created = DateTimeOffset.Now,
                IsLocked = false,
                IsPublic = false
            };
        }
    }
}
