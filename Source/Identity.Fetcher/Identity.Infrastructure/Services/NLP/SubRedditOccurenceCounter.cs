using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;

namespace Identity.Infrastructure.Services.NLP
{
    public class SubRedditOccurenceCounter
    {



        public IDictionary<string, long> FindOccurences(string subString)
        {
            var credential = GoogleCredential.GetApplicationDefaultAsync().Result;
            var projectId = "identity-jweibel-88507";

            var client = BigQueryClient.Create(projectId, credential);
            var table = client.GetTable("fh-bigquery", "reddit_posts", "full_corpus_201509");


            var xxx = @"#standardSQL
SELECT c.subreddit, e.Name, count(*) as cnt FROM `fh-bigquery.reddit_posts.full_corpus_201509` c
join `identity-jweibel-88507.Identity.NLPEntities2` e on c.title like Concat('%', e.Name, '%')
group by c.subreddit, e.Name";

            var xx = @"SELECT subreddit,
            case when title like '%html%' then 'html' when title like '%functional programming language%' then 'functional programming language' else NULL end as XX,
count(*) as cnt
FROM[fh - bigquery:reddit_posts.full_corpus_201509]
where title like '%html%' or title like '%functional programming language%'
group by subreddit, XX";

            var sql = $@"SELECT subreddit, count(*) as cnt
FROM `{table.FullyQualifiedId}`
where title like '%{subString}%'
group by subreddit
order by cnt desc";

            var result = client.ExecuteQuery(sql);

            return result.GetRows().Where(row => row["subreddit"] != null).ToDictionary(row => (string) row["subreddit"], row => (long) row["cnt"]);
        }
    }
}
