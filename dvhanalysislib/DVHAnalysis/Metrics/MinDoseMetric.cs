namespace DVHAnalysis
{
    public class MinDoseMetric : Metric
    {
        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = dvh.MinDose,
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }
    }
}
