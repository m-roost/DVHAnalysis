using System.ComponentModel;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class DVHMetricSetupViewModel : BindableBase
    {
        private DVHMetricSetup _dvhMetricSetup;
        public DVHMetricSetup DVHMetricSetup
        {
            get { return _dvhMetricSetup; }
            set
            {
                _dvhMetricSetup = value;
                NotifyPropertyChanged("DVHMetricSetup");
            }
        }

        public string Name
        {
            get { return PropertiesChanged ? GetChangedName() : DVHMetricSetup.Name; }
            set
            {
                DVHMetricSetup.Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Description
        {
            get { return DVHMetricSetup.Description; }
            set
            {
                DVHMetricSetup.Description = value;
                NotifyPropertyChanged("Description");
            }
        }

        private DVHModelViewModel _dvhModelViewModel;
        public DVHModelViewModel DVHModelViewModel
        {
            get { return _dvhModelViewModel; }
            set
            {
                _dvhModelViewModel = value;
                DVHMetricSetup.DVHModel = value.DVHModel;
                NotifyPropertyChanged("DVHModelViewModel");
            }
        }

        private MetricViewModel _metricViewModel;
        public MetricViewModel MetricViewModel
        {
            get { return _metricViewModel; }
            set
            {
                _metricViewModel = value;
                DVHMetricSetup.Metric = value.Metric;
                NotifyPropertyChanged("MetricViewModel");
            }
        }

        public bool IsAutoName
        {
            get { return DVHMetricSetup.IsAutoName; }
            set
            {
                DVHMetricSetup.IsAutoName = value;
                NotifyPropertyChanged("IsAutoName");
                NotifyPropertyChanged("Name");
            }
        }

        // Properties were changed by the user
        private bool _propertiesChanged;
        public bool PropertiesChanged
        {
            get { return _propertiesChanged; }
            set
            {
                _propertiesChanged = value;
                NotifyPropertyChanged("PropertiesChanged");
                NotifyPropertyChanged("Name");
            }
        }

        public DVHMetricSetupViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private string GetChangedName()
        {
            return DVHMetricSetup.Name + " [Changed]";
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DVHMetricSetup":    OnDVHMetricSetupChanged();    break;
                case "DVHModelViewModel": OnDVHModelViewModelChanged(); break;
                case "MetricViewModel":   OnMetricViewModelChanged();   break;
            }
        }

        private void OnDVHMetricSetupChanged()
        {
            DVHModelViewModel = DVHModelViewModel.CreateFromDVHModel(DVHMetricSetup.DVHModel);
            MetricViewModel = MetricViewModel.CreateFromMetric(DVHMetricSetup.Metric);
        }

        private void OnDVHModelViewModelChanged()
        {
            // If any property in the DVH model changes,
            // the name may have changed, so update it
            DVHModelViewModel.PropertyChanged += (s, e) => NotifyPropertyChanged("Name");
            SetDoseAndVolumeTypes();

            NotifyPropertyChanged("Name");
        }

        private void OnMetricViewModelChanged()
        {
            // If any property in the metric changes,
            // the name may have changed, so update it
            MetricViewModel.PropertyChanged += (s, e) => NotifyPropertyChanged("Name");
            SetDoseAndVolumeTypes();

            NotifyPropertyChanged("Name");
        }

        private void SetDoseAndVolumeTypes()
        {
            // NTCP or EUD must have absolute dose and volume
            if (MetricViewModel is NTCPMetricViewModel ||
                MetricViewModel is EUDMetricViewModel)
            {
                DVHModelViewModel.DoseType = DoseValuePresentation.Absolute;
                DVHModelViewModel.VolumeType = VolumePresentation.AbsoluteCm3;
            }
        }

        public MetricResult Calculate(EclipseData eclipseData)
        {
            return DVHMetricSetup.Calculate(eclipseData);
        }
    }
}
