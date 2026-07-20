namespace DVHAnalysis
{
    public class ColdVolumeWithDoseMetric : Metric
    {
        public double Dose { get; set; }

        public override MetricResult Calculate(DVH dvh)
        {
            // CV15Gy(cc) = Volume - V15Gy(cc)
            VolumeWithDoseMetric vdMetric = new VolumeWithDoseMetric()
            {
                Dose = Dose
            };

            MetricResult result = vdMetric.Calculate(dvh);
            result.Value = GetTotalVolume(dvh) - result.Value;
            return result;
        }

        private double GetTotalVolume(DVH dvh)
        {
            if (dvh.VolumeUnit == VolumeUnit.cc)
            {
                return dvh.TotalVolume;
            }
            else if (dvh.VolumeUnit == VolumeUnit.Percent)
            {
                return 100.0;
            }
            else
            {
                return 0.0;
            }
        }
    }
}
