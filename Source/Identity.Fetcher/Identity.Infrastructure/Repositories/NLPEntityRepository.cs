﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Identity.Domain;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Services.NLP;

namespace Identity.Infrastructure.Repositories
{
    public class NLPEntityRepository
    {
        private readonly IDbTransaction con;

        public NLPEntityRepository(IDbTransaction con)
        {
            this.con = con;
        }

        public IDictionary<string, long> All()
        {
            return con.Connection
                .Query("select Id, Name from NLPEntity", new {}, con)
                .Cast<IDictionary<string, object>>()
                .ToDictionary(row => (string) row["Name"], row => (long) row["Id"]);
            ;
        }

        public void Add(NLPEntity e)
        {
            var sql =
                @"insert NLPEntity (Name, Type, CommonWord, Noun) values (@Name, @Type, @CommonWord, @Noun); SELECT CAST(SCOPE_IDENTITY() as bigint)";
            e.Id = con.Connection.Query<long>(sql, e, con).Single();
        }

        public void EntitiesInPosts(IEnumerable<KeyValuePair<long, long>> entityIdsAndPostIds)
        {
            var table = new DataTable();
            table.TableName = "EntitiesInPosts";
            table.Columns.Add(new DataColumn("PostId", typeof(long)));
            table.Columns.Add(new DataColumn("NLPEntityId", typeof(long)));
            table.Columns.Add(new DataColumn("IdentifiedByGoogleNLP", typeof(bool)));

            var rows = entityIdsAndPostIds.Select(kv =>
            {
                var row = table.NewRow();
                row["PostId"] = kv.Value;
                row["NLPEntityId"] = kv.Key;
                row["IdentifiedByGoogleNLP"] = true;
                return row;
            });

            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            BulkCopy.Copy((SqlConnection) con.Connection, table, (SqlTransaction) con);
        }

        public IEnumerable<KeyValuePair<long, long>> EntitiesInPosts()
        {
            var sql = @"select PostId, NLPEntityId from EntitiesInPosts";
            return con.Connection.Query(sql, new {}, con)
                        .Cast<IDictionary<string, object>>()
                        .Select(row => new KeyValuePair<long, long>((long)row["PostId"], (long)row["NLPEntityId"]));
        }

        public IEnumerable<NLPEntity> Entities()
        {
            var sql = @"select e.Id, e.Name, e.Type from NLPEntity e";
            return con.Connection.Query<NLPEntity>(sql, new { }, con).ToList();
        }

        public IEnumerable<NLPEntity> Entities(long postId)
        {
            var sql = @"select e.Id, e.Name, e.Type
                        from EntitiesInPosts r join NLPEntity e on r.NLPEntityId = e.Id
                        where r.PostId = @PostId";
            return con.Connection.Query<NLPEntity>(sql, new {PostId = postId}, con).ToList();
        }

        public IEnumerable<Tuple<long, string>> Entities(IEnumerable<long> postIds)
        {
            var sql = @"select r.PostId as Item1, e.Name as Item2
                        from EntitiesInPosts r join NLPEntity e on r.NLPEntityId = e.Id
                        where r.PostId in @PostIds";
            return con.Connection.Query<Tuple<long, string>>(sql, new { PostIds = postIds }, con).ToList();
        }
    }
}