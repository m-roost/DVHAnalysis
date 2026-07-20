using System;
using System.Linq;

namespace DVHAnalysis
{
    public class EUDMetric : Metric
    {
        public double a { get; set; }

        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = CalculateValue(dvh),
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }

        private double CalculateValue(DVH dvh)
        {
            double EUDbSum = (from point in GetNonCumulativeData(dvh.CurveData)
                              where point.Dose > 0
                              select CalculateEUDt(point, dvh.TotalVolume)).Sum();
            return Math.Pow(EUDbSum, 1.0 / a);
        }

        private DVPoint[] GetNonCumulativeData(DVPoint[] cumulPoints)
        {
            DVPoint[] nonCumulPoints = new DVPoint[cumulPoints.Length];

            nonCumulPoints[nonCumulPoints.Length - 1] = new DVPoint
            {
                Dose = cumulPoints[cumulPoints.Length - 1].Dose,
                Volume = cumulPoints[cumulPoints.Length - 1].Volume
            };

            for (int i = 0; i < cumulPoints.Length - 1; i++)
            {
                nonCumulPoints[i] = new DVPoint
                {
                    Dose = cumulPoints[i].Dose,
                    Volume = cumulPoints[i].Volume - cumulPoints[i + 1].Volume
                };
            }

            return nonCumulPoints;
        }

        private double CalculateEUDt(DVPoint point, double volume)
        {
            return (point.Volume / volume) * Math.Pow(point.Dose, a);
        }
    }
}
