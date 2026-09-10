namespace DVHAnalysis
{
    public class DVHMetricSetup
    {
        private string _name;
        public string Name
        {
            get { return IsAutoName ? new MetricNamer(this).GetName() : _name; }
            set { _name = value; }
        }

        public bool IsAutoName { get; set; }
        public string Description { get; set; }
        public DVHModel DVHModel { get; set; }
        public Metric Metric { get; set; }

        public DVHMetricSetup()
        {
            IsAutoName = true;
        }

        public MetricResult Calculate(EclipseData eclipseData)
        {
            MetricResult result = Metric.Calculate(DVHModel.Calculate(eclipseData));
            result.EclipseData = eclipseData;
            result.DVHMetricSetup = this;
            return result;
        }
    }
}
