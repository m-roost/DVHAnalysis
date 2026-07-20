namespace DVHAnalysis
{
    /// <summary>
    /// This SMPC metric is not calculated by code. Instead, it is supposed to be user input (after their manual calculation outside of this DVH Analysis script.).
    /// </summary>
    public class SMPCMetric : Metric
    {
        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = double.NaN,
                Unit = MetricUnitConverter.FromDoseUnit(dvh.DoseUnit)
            };
        }
    }
}
