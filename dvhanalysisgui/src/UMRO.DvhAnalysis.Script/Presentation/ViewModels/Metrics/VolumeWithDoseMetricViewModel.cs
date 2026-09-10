using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class VolumeWithDoseMetricViewModel : MetricViewModel
    {
        public double Dose
        {
            get { return (Metric as VolumeWithDoseMetric).Dose; }
            set
            {
                (Metric as VolumeWithDoseMetric).Dose = value;
                NotifyPropertyChanged("Dose");
            }
        }
    }
}
