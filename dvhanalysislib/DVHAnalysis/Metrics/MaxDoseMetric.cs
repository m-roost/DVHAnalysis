namespace DVHAnalysis
{
    public class MaxDoseMetric : Metric
    {
        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = dvh.MaxDose,
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }
    }
}
