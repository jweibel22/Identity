﻿namespace Identity.Domain
{
    public class User
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public long SavedChannel { get; set; }

        public long StarredChannel { get; set; }

        public long LikedChannel { get; set; }

        public long Inbox { get; set; }

        public long SubscriptionChannel { get; set; }

        public string IdentityId { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsPremium { get; set; }
    }
}