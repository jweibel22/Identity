using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Identity.Domain.Clustering
{
    public static class VectorDistanceMeasure
    {
        static double CosineSim(Vector<double> s1, Vector<double> s2)
        {
            return s1.DotProduct(s2) / (s1.L2Norm() * s2.L2Norm());
        }

        public static double Get(Vector<double> v1, Vector<double> v2)
        {
            var sim = CosineSim(v1, v2);
            if (Double.IsNaN(sim))
            {
                throw new Exception("NaN");
            }
            if (sim == 0.0)
                return Double.MaxValue;
            else
                return (1 / sim) - 1;
        }
    }


    public static class DistanceMeasure
    {
        static double DotProduct(double[] s1, double[] s2)
        {
            return System.Linq.Enumerable.Zip(s1, s2, (a, b) => a * b).Sum();
        }

        public static double Magnitude(double[] v)
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
            if (Double.IsNaN(sim))
            {
                throw new Exception("NaN");
            }
            if (sim == 0.0)
                return Double.MaxValue;
            else
                return (1 / sim) - 1;
        }
    }
}