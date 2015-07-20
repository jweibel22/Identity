import org.apache.mahout.cf.taste.common.TasteException;
import org.apache.mahout.cf.taste.impl.similarity.AbstractItemSimilarity;
import org.apache.mahout.cf.taste.model.DataModel;
import org.apache.mahout.cf.taste.similarity.ItemSimilarity;
import org.apache.mahout.cf.taste.similarity.PreferenceInferrer;
import org.apache.mahout.cf.taste.similarity.UserSimilarity;

/**
 * Created by jwe on 02-05-2015.
 */
public class TagSimilarity extends AbstractItemSimilarity { //implements UserSimilarity {

    protected TagSimilarity(DataModel dataModel) {
        super(dataModel);
    }

    @Override
    public double itemSimilarity(long l, long l2) throws TasteException {
        return 0;
    }

    @Override
    public double[] itemSimilarities(long l, long[] longs) throws TasteException {
        return new double[0];
    }
/*
    @Override
    public double userSimilarity(long l, long l2) throws TasteException {
        return 0;
    }

    @Override
    public void setPreferenceInferrer(PreferenceInferrer preferenceInferrer) {

    }
*/
}

class CompositeSimilarity extends AbstractItemSimilarity {

    private ItemSimilarity s1;
    private ItemSimilarity s2;

    protected CompositeSimilarity(DataModel dataModel, ItemSimilarity s1, ItemSimilarity s2) {
        super(dataModel);
        this.s1 = s1;
        this.s2 = s2;
    }

    @Override
    public double itemSimilarity(long l, long l2) throws TasteException {
        return s1.itemSimilarity(l,l2) * s2.itemSimilarity(l,l2);
    }

    @Override
    public double[] itemSimilarities(long l, long[] longs) throws TasteException {
        return new double[0];
    }
}