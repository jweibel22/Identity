using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Helpers
{
    public class EnglishLanguage
    {
        public IDictionary<string, bool> CommonWords { get; }
        public IDictionary<string, bool> CommonNouns { get; }

        private string[] ignoredCharacters = new[] { ":", ",", ";", "»", "«", "'", "?", "!", "(", ")", "[", "]", "{", "}", "\"", "'", "#", "*", "~", "`", "…", "|", "“", "”", "-", "–", "_", "." };

        public EnglishLanguage(): this(@"Resources\commonwords-english.txt", @"Resources\nouns.csv")
        {
            
        }

        public EnglishLanguage(string commonWordsFile, string commonNounsFile)
        {
            CommonWords =
                System.IO.File.ReadAllLines(commonWordsFile)
                    .Select(w => w.Trim().ToLower())
                    .Take(1000)
                    .Distinct()
                    .ToDictionary(w => w, w => true);

            CommonNouns = System.IO.File.ReadAllLines(commonNounsFile)
                            .Select(w => w.Trim().ToLower()).Distinct().ToDictionary(s => s, s => true);
        }

        public string IgnoreAll(string s)
        {
            var result = s;
            foreach (var x in ignoredCharacters)
            {
                result = result.Replace(x, " ");
            }
            return result;
        }


        public IEnumerable<string> GetWords(string title)
        {
            //TODO: ignore all emojis. like: \ud83d\ude18            
            var ss = IgnoreAll(title.Trim());
            var words = ss.Replace("-", " ").Replace("–", " ").Replace("_", " ").Replace(".", " ")
                    .ToLower().Split(' ').Where(word => !String.IsNullOrEmpty(word) && word.Length > 2 && word.Length <= 100 && !CommonWords.ContainsKey(word));

            return words;
        }

        //public Dictionary<string, long> GetAllWords(SqlConnection con)
        //{
        //    var words = new Dictionary<string, long>();

        //    var cmd = new SqlCommand("select Id,Contents from Words", con);
        //    using (SqlDataReader reader = cmd.ExecuteReader())
        //    {
        //        while (reader.Read())
        //        {
        //            words[(string) reader["Contents"]] = (long) reader["Id"];
        //        }
        //    }
        //    return words;
        //}

        //public IEnumerable<string> ExtractWords(IEnumerable<string> texts)
        //{
        //    Dictionary<string, bool> seen = new Dictionary<string, bool>(100000);

        //    foreach (var t in texts)
        //    {
        //        var words = GetWords(t);
        //        foreach (var word in words)
        //        {
        //            seen[word] = true;
        //        }
        //    }

        //    return seen.Keys;
        //}

        //public void InsertWords(SqlConnection con, IEnumerable<string> texts)
        //{
        //    var existing = GetAllWords(con);
        //    var newWords = ExtractWords(texts);

        //    //Console.WriteLine("Found {0} items", newWords.Count());

        //    var table = new DataTable();
        //    table.TableName = "Words";
        //    table.Columns.Add(new DataColumn("Contents"));
        //    var idx = 0;

        //    foreach (var word in newWords.Where(w => !existing.ContainsKey(w)))
        //    {
        //        var row = table.NewRow();
        //        row["Contents"] = word;
        //        table.Rows.Add(row);

        //        if (++idx % 1000000 == 0)
        //        {
        //            BulkCopy.Copy(con, table);
        //            table.Clear();
        //        }
        //    }

        //    BulkCopy.Copy(con, table);
        //    table.Clear();

        //}
    }
}
