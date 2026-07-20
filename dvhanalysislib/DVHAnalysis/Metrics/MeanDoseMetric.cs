namespace DVHAnalysis
{
    public class MeanDoseMetric : Metric
    {
        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = dvh.MeanDose,
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }
    }
}
