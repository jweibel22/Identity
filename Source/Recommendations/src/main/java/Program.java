import org.apache.mahout.cf.taste.common.Refreshable;
import org.apache.mahout.cf.taste.common.TasteException;
import org.apache.mahout.cf.taste.eval.*;
import org.apache.mahout.cf.taste.impl.common.FastByIDMap;
import org.apache.mahout.cf.taste.impl.eval.AverageAbsoluteDifferenceRecommenderEvaluator;
import org.apache.mahout.cf.taste.impl.eval.GenericRecommenderIRStatsEvaluator;
import org.apache.mahout.cf.taste.impl.model.GenericBooleanPrefDataModel;
import org.apache.mahout.cf.taste.impl.model.file.FileDataModel;
import org.apache.mahout.cf.taste.impl.neighborhood.NearestNUserNeighborhood;
import org.apache.mahout.cf.taste.impl.recommender.AbstractRecommender;
import org.apache.mahout.cf.taste.impl.recommender.GenericBooleanPrefItemBasedRecommender;
import org.apache.mahout.cf.taste.impl.recommender.GenericItemBasedRecommender;
import org.apache.mahout.cf.taste.impl.recommender.GenericUserBasedRecommender;
import org.apache.mahout.cf.taste.impl.similarity.LogLikelihoodSimilarity;
import org.apache.mahout.cf.taste.impl.similarity.PearsonCorrelationSimilarity;
import org.apache.mahout.cf.taste.impl.similarity.TanimotoCoefficientSimilarity;
import org.apache.mahout.cf.taste.model.DataModel;
import org.apache.mahout.cf.taste.model.PreferenceArray;
import org.apache.mahout.cf.taste.neighborhood.UserNeighborhood;
import org.apache.mahout.cf.taste.recommender.CandidateItemsStrategy;
import org.apache.mahout.cf.taste.recommender.IDRescorer;
import org.apache.mahout.cf.taste.recommender.RecommendedItem;
import org.apache.mahout.cf.taste.recommender.Recommender;
import org.apache.mahout.cf.taste.similarity.ItemSimilarity;
import org.apache.mahout.cf.taste.similarity.UserSimilarity;

import java.io.File;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


/**
 * Created by jwe on 25-04-2015.
 */
    public class Program {

    public static void main(String[] args) throws IOException, TasteException {

        G(Integer.parseInt(args[0]), ReadPostMap());
        //G(2, ReadPostMap());
    }

    static Map<Integer, String> ReadPostMap() throws IOException {
        List<String> lines = Files.readAllLines(Paths.get("c:\\transit\\tmp\\postMap.csv"));
        Map<Integer,String> result = new HashMap<Integer, String>();

        for(String line : lines) {

            String id =line.substring(0, line.indexOf(":"));
            String name =line.substring(line.indexOf(":")+1);
            result.put(Integer.parseInt(id), name);
        }

        return result;
    }

    static void G(int userId, Map<Integer, String> postMap) throws IOException, TasteException
    {
        DataModel model = new FileDataModel(new File("c:\\transit\\tmp\\posts.csv"));

        DataModelBuilder modelBuilder = new DataModelBuilder() {
            @Override
            public DataModel buildDataModel(FastByIDMap<PreferenceArray> trainingData) {
                return new GenericBooleanPrefDataModel(
                        GenericBooleanPrefDataModel.toDataMap(trainingData));
            }
        };

        RecommenderBuilder recommenderBuilder = new RecommenderBuilder() {
            @Override
            public Recommender buildRecommender(DataModel model) throws TasteException {

                ItemSimilarity similarity = new LogLikelihoodSimilarity(model); //TanimotoCoefficientSimilarity(model); //
                return new GenericBooleanPrefItemBasedRecommender(model, similarity);
            }
        };

        GenericBooleanPrefItemBasedRecommender recommender =
                (GenericBooleanPrefItemBasedRecommender)recommenderBuilder.buildRecommender(model);
        List<RecommendedItem> recommendations = recommender.recommend(userId, 100);

        PrintWriter writer = new PrintWriter("c:\\transit\\tmp\\recommendations.txt", "UTF-8");

        for (RecommendedItem recommendation : recommendations) {
            writer.println(recommendation.getItemID() + ";" + recommendation.getValue());
        }
        writer.close();



/*
        RecommenderIRStatsEvaluator evaluator = new GenericRecommenderIRStatsEvaluator();
        //RecommenderEvaluator evaluator = new AverageAbsoluteDifferenceRecommenderEvaluator();
        IRStatistics stats = evaluator.evaluate(recommenderBuilder, modelBuilder, model, null, 10, 0.95, 0.95);
        System.out.println(stats.getPrecision());
        System.out.println(stats.getRecall());

        for (RecommendedItem recommendation : recommendations) {
            System.out.println(postMap.get(new Integer((int)recommendation.getItemID())) + " (" + recommendation.getValue() + ")");

            System.out.println("Because:");
            for(RecommendedItem item : recommender.recommendedBecause(userId,recommendation.getItemID(),5)) {
                System.out.println(String.format("%s (%s)", postMap.get(new Integer((int)item.getItemID())), item.getValue()));
            }
            System.out.println("---------------");
        }
*/
    }



}



