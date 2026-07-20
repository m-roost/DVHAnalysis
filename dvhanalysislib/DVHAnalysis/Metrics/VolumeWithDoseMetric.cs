namespace DVHAnalysis
{
    public class VolumeWithDoseMetric : Metric
    {
        public double Dose { get; set; }

        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = CalculateValue(dvh),
                Unit = MetricUnitConverter.FromVolumeUnit(dvh.VolumeUnit)
            };
        }

        private double CalculateValue(DVH dvh)
        {
            DVPoint[] points = dvh.CurveData;

            // Dose is past the histogram, so there can't be volume there
            if (Dose > points[points.Length - 1].Dose)
            {
                return 0.0;
            }

            int i = GetPointIndexWithAtMostDose(points);

            if (i == 0)
            {
                return points[0].Volume;
            }
            else
            {
                return InterpolateVolume(points[i - 1], points[i]);
            }
        }

        private int GetPointIndexWithAtMostDose(DVPoint[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Dose > Dose)
                {
                    return i;
                }
            }

            // TODO: Throw an exception, like "Desired dosePerFx too large";
            // for now return the last index, which is the point with the largest dosePerFx
            return points.Length - 1;
        }

        private double InterpolateVolume(DVPoint p1, DVPoint p2)
        {
            return p1.Volume + (Dose - p1.Dose) *
                (p2.Volume - p1.Volume) / (p2.Dose - p1.Dose);
        }
    }
}
