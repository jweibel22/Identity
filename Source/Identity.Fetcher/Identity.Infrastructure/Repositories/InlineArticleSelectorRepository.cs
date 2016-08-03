using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain;

namespace Identity.Infrastructure.Repositories
{
    public class InlineArticleSelectorRepository
    {
        private readonly Lazy<IEnumerable<InlineArticleSelector>> selectors;

        public InlineArticleSelectorRepository(IDbTransaction con)
        {
            selectors = new Lazy<IEnumerable<InlineArticleSelector>>(() => con.Connection.Query<InlineArticleSelector>("select * from InlineArticleSelector", new { }, con).ToList());
        }

        public IEnumerable<InlineArticleSelector> GetAll()
        {
            return selectors.Value;
        }
    }
}
