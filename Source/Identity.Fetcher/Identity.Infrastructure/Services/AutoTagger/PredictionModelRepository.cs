using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Identity.Infrastructure.Services.AutoTagger
{
    public class PredictionModelRepository
    {
        private readonly IDbTransaction con;

        public PredictionModelRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public SerializedPredictionModel GetModel(long id)
        {
            return con.Connection.Query<SerializedPredictionModel>("select Id, Name, RedditIndexId, Model, SubRedditIds from AutoTagger_PredictionModel where Id = @id", new { Id = id }, con).SingleOrDefault();
        }

        public void AddModel(SerializedPredictionModel model)
        {
            model.Id = con.Connection.Query<long>("insert AutoTagger_PredictionModel (Name, RedditIndexId, Model, SubRedditIds) values(@Name, @RedditIndexId, @Model, @SubRedditIds); SELECT CAST(SCOPE_IDENTITY() as bigint)", model, con).Single();
        }
    }
}
