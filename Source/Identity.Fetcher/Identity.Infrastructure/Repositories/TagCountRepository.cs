using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;

namespace Identity.Infrastructure.Repositories
{
    public class TagCount
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }
    }

    public class TagCountRepository
    {
        private readonly IDbTransaction con;

        public TagCountRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public IEnumerable<TagCount> GetTagCountsContainingWords(IEnumerable<string> words)
        {
            var sql = @"select top 10 t.Id, t.Name, count(*) as Count from PostTitleWords ptw
join Word w on w.Id = ptw.WordId
join Tagged tg on tg.PostId = ptw.PostId
join Tag t on t.Id = tg.TagId
where w.Contents in (@Words)
group by t.Id, t.Name
order by count(*) desc";

            return con.Connection.Query<TagCount>(sql, new { Words = words }, con);
        }

        public IEnumerable<TagCount> GetTagsCountsByIds(IEnumerable<long> tagIds)
        {
            var sql = @"select t.Id, t.Name, count(*) as Count from PostTitleWords ptw
join Tagged tg on tg.PostId = ptw.PostId
join Tag t on t.Id = tg.TagId
where t.Id in (@TagIds)
group by t.Id, t.Name";

            return con.Connection.Query<TagCount>(sql, new { TagIds = tagIds }, con);
        }
    }
}
