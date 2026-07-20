using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DVHAnalysis.UnitTests
{
    [TestFixture]
    public class DoseVolumeHistogramUnitTests
    {
        [Test]
        public void SmallTestCase_Randomized()
        {
            var dose = GetRandomDoubles(2,  0.1, 0.2)
               .Concat(GetRandomDoubles(10, 0.2, 0.3)
               .Concat(GetRandomDoubles(15, 0.3, 0.4)
               .Concat(GetRandomDoubles(13, 0.4, 0.5)
               .Concat(GetRandomDoubles(8,  0.5, 0.6)
               .Concat(GetRandomDoubles(5,  0.6, 0.7))))));

            var dvh = new DoseVolumeHistogram(dose.ToArray(), 1.0, 0.1);

            Assert.AreEqual(53.0, dvh.Curve[0].Volume);
            Assert.AreEqual(53.0, dvh.Curve[1].Volume);
            Assert.AreEqual(51.0, dvh.Curve[2].Volume);
            Assert.AreEqual(41.0, dvh.Curve[3].Volume);
            Assert.AreEqual(26.0, dvh.Curve[4].Volume);
            Assert.AreEqual(13.0, dvh.Curve[5].Volume);
            Assert.AreEqual(5.0,  dvh.Curve[6].Volume);
        }

        private IEnumerable<double> GetRandomDoubles(int n, double min, double max)
        {
            return GetRandomDoubles(min, max).Take(n);
        }

        private IEnumerable<double> GetRandomDoubles(double min, double max)
        {
            while (true)
            {
                yield return GetRandomDouble(min, max);
            }
        }

        private double GetRandomDouble(double min, double max)
        {
            return _random.NextDouble() * (max - min) + min;
        }

        private readonly Random _random = new Random();
    }
}
