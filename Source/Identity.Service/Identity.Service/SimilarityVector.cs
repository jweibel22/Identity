using System.Collections.Generic;
using System.Linq;

namespace Identity.Service
{
    class SimilarityVector
    {
        private IDictionary<int, double> items;

        public SimilarityVector(IDictionary<int, double> items)
        {
            this.items = items;
        }

        public SimilarityVector()
        {
            items = new Dictionary<int, double>();
        }

        public SimilarityVector(IEnumerable<int> items)
        {
            this.items = items.ToDictionary(item => item, item => 1.0);
        }

        public double For(int itemId)
        {
            return items.ContainsKey(itemId) ? items[itemId] : 0.0;
        }

        public void SetFor(int itemId, double value)
        {
            items[itemId] = value;
        }

        public void Add(IEnumerable<int> itemIds)
        {
            foreach (var itemId in itemIds)
            {
                if (!items.ContainsKey(itemId))
                {
                    items.Add(itemId, 1);
                }
                else
                {
                    items[itemId] = items[itemId] + 1;
                }
            }
        }

        public IEnumerable<int> ItemIds
        {
            get { return items.Keys; }
        }
    }
}