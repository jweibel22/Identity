using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.IO;
using Accord.MachineLearning.Bayes;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Univariate;
using Identity.Domain.RedditIndexes;
using Identity.Infrastructure.Reddit;

namespace Identity.Infrastructure.Services.AutoTagger
{
    public class SerializedPredictionModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long RedditIndexId { get; set; }

        public byte[] Model { get; set; }

        public string SubRedditIds { get; set; }

    }

    public class PredictionModel
    {
        public long Id { get; set; }

        public NaiveBayes<NormalDistribution> Model { get; set; }
        public IList<long> SubReddits { get; set; }
        public string Name { get; set; }
        private FullRedditIndex Index { get; set; }
        private FeatureExtractor FeatureExtractor { get; set; }

        public SerializedPredictionModel Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Save(Model, ms);

                return new SerializedPredictionModel
                {
                    Id = Id,
                    Name = Name,
                    RedditIndexId = Index.Id,
                    Model = ms.ToArray(),
                    SubRedditIds = String.Join(",", SubReddits)
                };
            }
        }

        public static PredictionModel Load(SerializedPredictionModel model, FeatureExtractor featureExtractor, RedditIndexRepository repo)
        {
            return new PredictionModel
            {
                Id = model.Id,
                FeatureExtractor = featureExtractor,
                Index = repo.GetFullRedditIndex(model.RedditIndexId),
                Name = model.Name,
                SubReddits = model.SubRedditIds.Split(',').Select(s => Int64.Parse(s)).ToList(),
                Model = Serializer.Load<NaiveBayes<NormalDistribution>>(model.Model)
            };
        }

        public static PredictionModel Train(FullRedditIndex index, FeatureExtractor featureExtractor, string name, IList<PostAndId> trainingPosts)
        {

            var subredditNames = index.SubReddits.ToDictionary(sr => sr.Id, sr => sr.Name);
            var temp = featureExtractor.ComputeFeatureVectors(trainingPosts);
            var vocabulary = temp.SelectMany(d => d.Keys).Distinct().ToList();
            var trainingFeatures = temp.Select(d => vocabulary.Select(l => d.ContainsKey(l) ? d[l] : 0.0).ToArray()).ToArray();
            var trainingLabels = trainingPosts.Select(p => (int)p.Id).ToArray();

            //PrintToCsv(articleNames, subRedditNames, suggestions);

            var learner = new NaiveBayesLearning<NormalDistribution>();
            learner.Options.InnerOption = new NormalOptions
            {
                Regularization = 1e-5 // to avoid zero variances
            };

            return new PredictionModel
            {
                Name = name,
                Model = learner.Learn(trainingFeatures, trainingLabels),
                SubReddits = vocabulary,
                Index = index,
                FeatureExtractor = featureExtractor
            };
        }

        public int[] Decide(IList<PostAndId> testPosts)
        {
            var testFeatures = FeatureExtractor
                .ComputeFeatureVectors(testPosts)
                .Select(d => SubReddits.Select(l => d.ContainsKey(l) ? d[l] : 0.0).ToArray()).ToArray();
            return Model.Decide(testFeatures);
        }

        public int[] Decide(IList<long> testPosts)
        {
            var featureVector = FeatureExtractor.ComputeFeatureVectors(testPosts);
               var xx = SubReddits.Select(l => featureVector.ContainsKey(l) ? featureVector[l] : 0.0).ToArray();
            return Model.Decide(new[] {xx });
        }
    }
}