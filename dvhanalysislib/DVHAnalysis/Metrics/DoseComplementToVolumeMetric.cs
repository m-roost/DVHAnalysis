namespace DVHAnalysis
{
    public class DoseComplementToVolumeMetric : Metric
    {
        public double Volume { get; set; }

        public override MetricResult Calculate(DVH dvh)
        {
            // DCx%(Gy) = D(Volume - x%)(Gy)
            DoseToVolumeMetric dvMetric = new DoseToVolumeMetric
            {
                Volume = GetTotalVolume(dvh) - Volume
            };

            return dvMetric.Calculate(dvh);
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
