using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Domain.Clustering
{
    static class DistanceMeasure
    {
        static double DotProduct(double[] s1, double[] s2)
        {
            return System.Linq.Enumerable.Zip(s1, s2, (a, b) => a * b).Sum();
        }

        static double Magnitude(double[] v)
        {
            return Math.Sqrt(v.Select(x => x * x).Sum());
        }

        static double CosineSim(double[] s1, double[] s2)
        {
            return DotProduct(s1, s2) / (Magnitude(s1) * Magnitude(s2));
        }

        public static double Get(double[] v1, double[] v2)
        {
            var sim = CosineSim(v1, v2);
            if (sim == 0.0)
                return Double.MaxValue;
            else
                return (1 / sim) - 1;
        }
    }
}