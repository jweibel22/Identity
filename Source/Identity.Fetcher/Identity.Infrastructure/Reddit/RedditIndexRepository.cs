using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Helpers;

namespace Identity.Infrastructure.Reddit
{
    public class RedditIndexRepository
    {
        private readonly IDbTransaction con;

        readonly string[] ignoredReddits = new[]
        {
                "blog",
                "GlobalOffensiveTrade",
                "RocketLeagueExchange",
                "videos",
                "Fireteams",
                "news",
                "AskReddit",
                "Jokes",
                "funny",
                "aww",
                "pics",
                "worldnews",
                "gifs",
                "todayilearned",
                "HOTandTRENDING",
                "EarthPorn",
                "IAmA",
                "announcements",
                "reddit.com"
            };

        public RedditIndexRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public void AddRedditIndex(RedditIndex index)
        {
            index.Id =
                con.Connection.Query<long>(
                    "insert RedditIndex_Index (StorageLocation) values (@StorageLocation); SELECT CAST(SCOPE_IDENTITY() as bigint)",
                    index, con).Single();

            var table = new DataTable();
            table.TableName = "RedditIndex_SubReddit";
            table.Columns.Add(new DataColumn("Name", typeof(string)));
            table.Columns.Add(new DataColumn("IndexId", typeof(long)));
            table.Columns.Add(new DataColumn("PostCount", typeof(int)));

            var rows = index.SubReddits.Select(x =>
            {
                var row = table.NewRow();
                row["Name"] = x.Name;
                row["PostCount"] = x.PostCount;
                row["IndexId"] = index.Id;
                return row;
            });

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            BulkCopy.Copy((SqlConnection) con.Connection, table, (SqlTransaction) con);

            con.Connection.Execute(@"update RedditIndex_SubReddit set Ignore = 1 where Name in @IgnoredSubReddits", new { IgnoredSubReddits  = ignoredReddits }, con);
            con.Connection.Execute(@"update RedditIndex_SubReddit set Ignore = 1 where Name like '%nsfw%' or Name like '%sex%'", new { }, con);

            //TODO: update the ids of the subreddits on the index object
        }

        public void DeleteRedditIndex(long id)
        {
            con.Connection.Execute("delete from RedditIndex_Occurences where IndexId=@Id", new { Id = id }, con);
            con.Connection.Execute("delete from RedditIndex_Text where IndexId=@Id", new { Id = id }, con);
            con.Connection.Execute("delete from RedditIndex_SubReddit where IndexId=@Id", new { Id = id }, con);
            con.Connection.Execute("delete from RedditIndex_Index where Id=@Id", new { Id = id }, con);
        }

        public RedditIndex GetRedditIndex(long id)
        {
            var index = con.Connection.Query<RedditIndex>("select * from RedditIndex_Index where Id=@Id", new { Id = id }, con).SingleOrDefault();

            if (index != null)
            {
                index.SubReddits = con.Connection
                    .Query<SubReddit>("select Id, Name, PostCount from RedditIndex_SubReddit where IndexId=@Id", new { Id = id }, con).ToList();

                index.TotalPostCount =
                    con.Connection.Query<long>(
                        "select sum(PostCount) from RedditIndex_SubReddit where IndexId = @IndexId", new {IndexId = id},
                        con).Single();
            }

            return index;
        }

        public Text GetText(long indexId, string text)
        {
            return con.Connection
                .Query<Text>("select * from RedditIndex_Text where IndexId=@IndexId and Content=@Text", new { IndexId = indexId, Text = text }, con)
                .SingleOrDefault();
        }


        public void AddText(long indexId, Text text)
        {
            var sql = "insert RedditIndex_Text (IndexId, Content, Type, CommonWord, Noun) values (@IndexId, @Content, @Type, 0, 0); SELECT CAST(SCOPE_IDENTITY() as bigint)";
            text.Id = con.Connection.Query<long>(sql, new { IndexId = indexId, Content = text.Content, Type = text.Type }, con).Single();
        }

        public void AddOccurences(IEnumerable<Occurences> occurences)
        {
            var table = new DataTable();
            table.TableName = "RedditIndex_Occurences";
            table.Columns.Add(new DataColumn("TextId", typeof(long)));
            table.Columns.Add(new DataColumn("SubRedditId", typeof(long)));
            table.Columns.Add(new DataColumn("IndexId", typeof(long)));
            table.Columns.Add(new DataColumn("Occurences", typeof(int)));

            var rows = occurences.Select(x =>
            {
                var row = table.NewRow();
                row["TextId"] = x.TextId;
                row["SubRedditId"] = x.SubRedditId;
                row["IndexId"] = x.IndexId;
                row["Occurences"] = x.Count;
                return row;
            });

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            BulkCopy.Copy((SqlConnection)con.Connection, table, (SqlTransaction)con);
        }

        public IEnumerable<Occurences> FindOccurences(long indexId, IEnumerable<long> textIds)
        {
            var sql = @"select r.TextId, r.SubRedditId, r.IndexId, r.Occurences as [Count] from RedditIndex_Occurences r
join RedditIndex_SubReddit sr on sr.Id = r.SubRedditId
where r.IndexId=@IndexId and r.TextId in @TextIds and sr.PostCount > 10 and sr.Ignore = 0";

            return con.Connection
                .Query<Occurences>(sql, new { IndexId = indexId, TextIds = textIds }, con)
                .ToList();
        }
    }
}
