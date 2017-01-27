using System.Collections.Generic;
using System.Linq;

namespace Identity.Infrastructure.Services.NLP
{
    public class Entity
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }

    public class Entities
    {
        public IDictionary<Text, List<Entity>> Articles { get; set; }

        public Entities(IList<Text> texts)
        {
            Articles = texts.ToDictionary(t => t, t => new List<Entity>());
        }

        public void Tag(Text article, Entity tag)
        {
            Articles[article].Add(tag);
        }
    }
}