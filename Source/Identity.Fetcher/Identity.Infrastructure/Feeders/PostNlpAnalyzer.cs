using System.Collections.Generic;
using System.Linq;
using Identity.Domain;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services.NLP;
using MoreLinq;

namespace Identity.Infrastructure.Feeders
{
    public class PostNlpAnalyzer
    {        
        private readonly GoogleNLPClient client;
        private readonly EnglishLanguage helper;     
        private readonly NLPEntityRepository repo;

        public PostNlpAnalyzer(NLPEntityRepository repo, EnglishLanguage helper, GoogleNLPClient client)
        {
            this.repo = repo;
            this.helper = helper;
            this.client = client;
        }

        public void AnalyzePosts(IList<Post> posts)
        {
            var existingEntities = repo.All();
            var items = posts.ToDictionary(a => new Text {Content = a.Title}, a => a);
            var articles = new Dictionary<Text, List<Entity>>();

            foreach (var kv in client.Get(items.Keys.ToList()).Articles)
            {
                articles.Add(kv.Key, kv.Value.Where(s => s.Name.Length > 1).ToList());
            }

            var toInsert = articles
                .SelectMany(kv => kv.Value)
                .DistinctBy(e => e.Name)
                .Where(e => !existingEntities.ContainsKey(e.Name))
                .Select(e => new NLPEntity
                {
                    Name = e.Name,
                    Type = e.Type,
                    CommonWord = helper.CommonWords.ContainsKey(e.Name),
                    Noun = helper.CommonNouns.ContainsKey(e.Name)
                });

            foreach (var e in toInsert)
            {
                repo.Add(e);
                existingEntities.Add(e.Name, e.Id);
            }

            var xx = new Dictionary<long, long>();

            foreach (var kv in articles)
            {
                var postId = items[kv.Key].Id;

                foreach (var s in kv.Value.DistinctBy(e => e.Name))
                {
                    var entityId = existingEntities[s.Name];
                    xx.Add(entityId, postId);
                }
            }

            repo.AddXX(xx);
        }

    }
}