using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public class DoseComplementToVolumeMetricViewModel : MetricViewModel
    {
        public double Volume
        {
            get { return (Metric as DoseComplementToVolumeMetric).Volume; }
            set
            {
                (Metric as DoseComplementToVolumeMetric).Volume = value;
                NotifyPropertyChanged("Volume");
            }
        }
    }
}
