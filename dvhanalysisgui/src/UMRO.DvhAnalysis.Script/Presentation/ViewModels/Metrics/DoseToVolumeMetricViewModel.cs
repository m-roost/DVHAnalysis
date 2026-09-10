using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class DoseToVolumeMetricViewModel : MetricViewModel
    {
        public double Volume
        {
            get { return (Metric as DoseToVolumeMetric).Volume; }
            set
            {
                (Metric as DoseToVolumeMetric).Volume = value;
                NotifyPropertyChanged("Volume");
            }
        }
    }
}
