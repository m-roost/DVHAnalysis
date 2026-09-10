using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVHAnalysis
{
    public static class Statistics
    {
        public static double StdDev(double[] data)
        {
            return Math.Sqrt(Variance(data));
        }

        private static double Variance(double[] data)
        {
            return SquaredDeviations(data).Average();
        }

        private static double[] SquaredDeviations(double[] data)
        {
            double mean = data.Average();
            return data.Select(d => Square(d - mean)).ToArray();
        }

        private static double Square(double x)
        {
            return x*x;
        }

        public static double Median(double[] data)
        {
            double[] sortedData = GetSorted(data);
            int mi = data.Length/2;    // Middle index
            return IsOdd(mi) ? sortedData[mi] : (sortedData[mi - 1] + sortedData[mi])/2.0;
        }

        private static double[] GetSorted(double[] data)
        {
            double[] sortedData = new double[data.Length];
            Array.Copy(data, sortedData, data.Length);
            Array.Sort(sortedData);
            return sortedData;
        }

        private static bool IsOdd(int i)
        {
            return i%2 == 1;
        }

        // Returns the probability that the observed value
        // of a standard normal random variable will be less than or equal to d
        public static double NormSDist(double d)
        {
            double erfHolder = Erf(d / Math.Sqrt(2.0));
            return (1.0 + erfHolder) / 2.0;
        }

        // Algorithm obtained from http://www.johndcook.com/blog/cpp_erf
        private static double Erf(double x)
        {
            // constants
            const double a1 =  0.254829592;
            const double a2 = -0.284496736;
            const double a3 =  1.421413741;
            const double a4 = -1.453152027;
            const double a5 =  1.061405429;
            const double p =   0.3275911;

            // Save the sign of x
            int sign = (x < 0) ? -1 : 1;

            x = Math.Abs(x);

            // Abramowitz and Stegun formula 7.1.26
            double t = 1.0/(1.0 + p*x);
            double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

            return sign*y;
        }
    }
}
