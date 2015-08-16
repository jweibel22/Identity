namespace Identity.Domain
{
    public class User
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public long SavedChannel { get; set; }

        public long StarredChannel { get; set; }

        public long LikedChannel { get; set; }

        public string IdentityId { get; set; }
    }
}