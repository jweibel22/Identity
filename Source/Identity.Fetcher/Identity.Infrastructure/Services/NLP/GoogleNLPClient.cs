using System;
using System.Collections.Generic;
using System.Linq;
using Identity.Infrastructure.Helpers;
using Newtonsoft.Json;
using RestSharp;

namespace Identity.Infrastructure.Services.NLP
{
    public class GoogleNLPClient
    {
        class Body
        {
            public Document document { get; set; }
            public string encodingType { get; set; }

            public static Body New(string content)
            {
                return new Body
                {
                    encodingType = "UTF16",
                    document = new Document
                    {
                        type = "PLAIN_TEXT",
                        content = content
                    }
                };
            }
        }

        class Document
        {
            public string type { get; set; }
            public string content { get; set; }
        }

        class ApiResponse
        {
            public IList<Entity> entities { get; set; }
            public string language { get; set; }
        }

        class Entity
        {
            public string name { get; set; }
            public string type { get; set; }
            public MetaData metadata { get; set; }
            public double salience { get; set; }
            public IList<Mention> mentions { get; set; }
        }

        class MetaData
        {
            public string mid { get; set; }
            public string wikipedia_url { get; set; }
        }

        class Mention
        {
            public string type { get; set; }
            public MentionText text { get; set; }
        }

        class MentionText
        {
            public string content { get; set; }
            public int beginOffset { get; set; }
        }


        private int IndexAtOffset(IList<Text> articles, int offset)
        {
            var separatorLength = 3;
            int idx = 0;
            int currentOffset = 0;

            while (idx < articles.Count && currentOffset + articles[idx].Content.Length + separatorLength <= offset)
            {
                currentOffset += articles[idx].Content.Length + separatorLength;
                idx++;
            }
            return idx;
        }

        private readonly string apiKey;
        private readonly EnglishLanguage helper;
        private readonly IRestClient client;

        public GoogleNLPClient(string apiKey, string url, EnglishLanguage helper)
        {
            this.apiKey = apiKey;
            this.helper = helper;
            this.client = new RestClient(url);
        }

        public Entities Get(IList<Text> texts)
        {            
            var request = new RestRequest("", Method.POST);
            request.AddQueryParameter("key", apiKey);
            request.RequestFormat = DataFormat.Json;

            var content = String.Join(" | ", texts.Select(a => a.Content));      
            var body = Body.New(content);
            request.AddBody(body);

            var response = client.Execute(request);
            var r = JsonConvert.DeserializeObject<ApiResponse>(response.Content);

            var result = new Entities(texts);

            foreach (var x in r.entities)
            {
                foreach (var mention in x.mentions)
                {
                    if (helper.CommonWords.Keys.Contains(mention.text.content.ToLower().Trim()))
                    {                        
                        continue;
                    }

                    var idx = IndexAtOffset(texts, mention.text.beginOffset);
                    if (idx < texts.Count)
                    {
                        result.Tag(texts[idx], new NLP.Entity { Name = mention.text.content.ToLower(), Type = x.type });
                    }
                }
            }

            return result;
        }
    }
}
