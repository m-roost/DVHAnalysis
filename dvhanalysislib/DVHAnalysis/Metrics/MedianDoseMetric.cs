namespace DVHAnalysis
{
    public class MedianDoseMetric : Metric
    {
        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = dvh.MedianDose,
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }
    }
}