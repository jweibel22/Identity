using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class Channel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public bool IsPublic { get; set; }

        public bool IsLocked { get; set; }

        public bool ShowOnlyUnread { get; set; }

        public string OrderBy { get; set; }

        public string ListType { get; set; }

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
                Created = DateTimeOffset.Now,
                IsLocked = false,
                IsPublic = false,
                ListType = "Full",
                OrderBy = "Added",
                ShowOnlyUnread = false
            };
        }

        public static Channel Standard(string name, bool streaming)
        {
            return new Channel
            {
                Created = DateTimeOffset.Now,
                Name = name,
                ShowOnlyUnread = !streaming,
                IsLocked = true,
                IsPublic = false,
                OrderBy = streaming ? "Popularity" : "Added",
                ListType = streaming ? "List" : "Full"
            };
        }
    }
}
