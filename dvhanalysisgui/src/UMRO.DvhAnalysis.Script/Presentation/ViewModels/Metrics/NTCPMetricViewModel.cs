using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class NTCPMetricViewModel : MetricViewModel
    {
        public double LKBn
        {
            get { return (Metric as NTCPMetric).LKBn; }
            set
            {
                (Metric as NTCPMetric).LKBn = value;
                NotifyPropertyChanged("LKBn");
            }
        }

        public double LKBm
        {
            get { return (Metric as NTCPMetric).LKBm; }
            set
            {
                (Metric as NTCPMetric).LKBm = value;
                NotifyPropertyChanged("LKBm");
            }
        }

        public double LKBD50
        {
            get { return (Metric as NTCPMetric).LKBD50; }
            set
            {
                (Metric as NTCPMetric).LKBD50 = value;
                NotifyPropertyChanged("LKBD50");
            }
        }
    }
}
