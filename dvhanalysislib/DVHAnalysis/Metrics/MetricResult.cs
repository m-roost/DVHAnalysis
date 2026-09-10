namespace DVHAnalysis
{
    public class MetricResult
    {
        public EclipseData EclipseData { get; set; }    // Data used by the DVHModel
        public DVHMetricSetup DVHMetricSetup { get; set; }
        public DVHModel DVHModel { get; set; }          // DVHModel used by the Metric
        public Metric Metric { get; set; }              // Metric used to generate result
        public double Value { get; set; }
        public MetricUnit Unit { get; set; }
    }
}
