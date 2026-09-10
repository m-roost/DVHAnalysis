using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class EUDMetricViewModel : MetricViewModel
    {
        public double a
        {
            get { return (Metric as EUDMetric).a; }
            set
            {
                (Metric as EUDMetric).a = value;
                NotifyPropertyChanged("a");
            }
        }
    }
}
