namespace Identity.Domain
{
    public class NLPEntity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool CommonWord { get; set; }

        public bool Noun { get; set; }
    }
}