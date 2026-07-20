using System;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.Converters;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics
{
    public abstract class MetricViewModel : BindableBase
    {
        private MetricViewModelTypeConverter metricTypeConverter =
            new MetricViewModelTypeConverter();

        private Metric _metric;
        public Metric Metric
        {
            get { return _metric; }
            set
            {
                _metric = value;
                NotifyPropertyChanged("Metric");
            }
        }

        public string TypeName
        {
            get
            {
                return (string)metricTypeConverter.Convert(this, typeof(string), null, null);
            }
        }

        public static MetricViewModel CreateFromMetric(Metric metric)
        {
            MetricViewModel metricViewModel = CreateDefaultFromMetric(metric);
            metricViewModel.Metric = metric;
            return metricViewModel;
        }

        public static MetricViewModel CreateDefaultFromMetric(Metric metric)
        {
            if (metric is MeanDoseMetric)
            {
                return new MeanDoseMetricViewModel();
            }
            else if (metric is MinDoseMetric)
            {
                return new MinDoseMetricViewModel();
            }
            else if (metric is MaxDoseMetric)
            {
                return new MaxDoseMetricViewModel();
            }
            else if (metric is StdDevDoseMetric)
            {
                return new StdDevDoseMetricViewModel();
            }
            else if (metric is DoseToVolumeMetric)
            {
                return new DoseToVolumeMetricViewModel();
            }
            else if (metric is VolumeWithDoseMetric)
            {
                return new VolumeWithDoseMetricViewModel();
            }
            else if (metric is ColdVolumeWithDoseMetric)
            {
                return new ColdVolumeWithDoseMetricViewModel();
            }
            else if (metric is DoseComplementToVolumeMetric)
            {
                return new DoseComplementToVolumeMetricViewModel();
            }
            else if (metric is NTCPMetric)
            {
                return new NTCPMetricViewModel();
            }
            else if (metric is EUDMetric)
            {
                return new EUDMetricViewModel();
            }
            else if (metric is SMPCMetric)
            {
                return new SMPCMetricViewModel();
            }
            else
            {
                throw new ApplicationException("Unknown metric type.");
            }
        }
    }
}
