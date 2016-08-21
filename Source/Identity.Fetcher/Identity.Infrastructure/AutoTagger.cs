using Identity.Domain;
using Identity.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure
{
    public class AutoTagger
    {
        class TagFrequency
        {
            public long Id { get; set; }

            public decimal Frequency { get; set; }
        }

        private readonly TagCountRepository tagCountrepo;
        private readonly PostRepository postRepo;

        public AutoTagger(TagCountRepository tagCountrepo, PostRepository postRepo)
        {
            this.tagCountrepo = tagCountrepo;
            this.postRepo = postRepo;
        }

        public void AutoTag(Post post)
        {
            var tagCounts = tagCountrepo.GetTagCountsContainingWords(post.TokenizedTitle);
            var totalTagCounts = tagCountrepo.GetTagsCountsByIds(tagCounts.Select(tc => tc.Id));
            var frequencies = tagCounts
                .Join(totalTagCounts, x => x.Id, x => x.Id, (x, y) => new TagFrequency
            {
                Id = x.Id,
                Frequency = (decimal)x.Count / (decimal)y.Count
            })
                .Where(f => f.Frequency > 0.05M)
                .ToList();

            foreach(var tag in frequencies)
            {
                postRepo.AutoTagPost(post.Id, tag.Id);
            }
        }
    }
}
