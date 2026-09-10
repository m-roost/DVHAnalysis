using System;
namespace DVHAnalysis
{
    public class DoseToVolumeMetric : Metric
    {
        public double Volume { get; set; }

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
            DVPoint[] points = dvh.CurveData;

            if (Volume > points[0].Volume)
            {
                throw new ApplicationException(GetVolumeErrorMessage(dvh));
            }

            int i = GetPointIndexWithAtLeastVolume(points);

            if (i == points.Length - 1)
            {
                return points[points.Length - 1].Dose;
            }
            else
            {
                return InterpolateDose(points[i], points[i + 1]);
            }
        }

        private string GetVolumeErrorMessage(DVH dvh)
        {
            return string.Format(
                "The metric's volume parameter ({0:f2}) should not be larger " +
                "than the first point in the structure's DVH curve ({1:f2}). " +
                "This means that the structure's volume is smaller " +
                "than the volume specified by the metric, or " +
                "the DVH curve is incorrect.", Volume, dvh.CurveData[0].Volume);
        }

        private int GetPointIndexWithAtLeastVolume(DVPoint[] points)
        {
            for (int i = points.Length - 1; i >= 0; i--)
            {
                if (points[i].Volume >= Volume)
                {
                    return i;
                }
            }

            // TODO: Throw an exception, like "Desired volume too large";
            // for now return the first index, which is the point with the largest volume
            return 0;
        }

        private double InterpolateDose(DVPoint p1, DVPoint p2)
        {
            return p1.Dose + (Volume - p1.Volume) *
                (p2.Dose - p1.Dose) / (p2.Volume - p1.Volume);
        }
    }
}
