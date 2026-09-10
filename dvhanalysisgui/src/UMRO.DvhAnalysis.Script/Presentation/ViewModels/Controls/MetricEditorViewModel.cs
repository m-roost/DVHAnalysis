using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.Commands;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    // TODO: Move these enums to their own files
    public enum MetricTypes
    {
        MeanDose,
        MinDose,
        MaxDose,
        StdDevDose,
        DoseToVolume,
        VolumeWithDose,
        ColdVolumeWithDose,
        DoseComplementToVolume,
        NTCP,
        gEUD,
        SMPC
    }

    public enum DVHModelTypes
    {
        Standard,
        LQBioDose,
        LQLBioDose
    }

    public class MetricEditorViewModel : BindableBase
    {
        private ObservableCollection<DVHMetricSetupViewModel> _dvhMetricSetups;
        public ObservableCollection<DVHMetricSetupViewModel> DVHMetricSetups
        {
            get { return _dvhMetricSetups; }
            set
            {
                _dvhMetricSetups = value;
                NotifyPropertyChanged("DVHMetricSetups");
            }
        }

        private DVHMetricSetupViewModel _selectedDVHMetricSetup;
        public DVHMetricSetupViewModel SelectedDVHMetricSetup
        {
            get { return _selectedDVHMetricSetup; }
            set
            {
                _selectedDVHMetricSetup = value;
                NotifyPropertyChanged("SelectedDVHMetricSetup");

                if (value != null)
                {
                    // SelectedMetricType and SelectedDVHModelType are not properties
                    // of SelectedDVHMetricSetup, so the binding system must be notified
                    // that they've changed everytime the SelectedDVHMetricSetup changes
                    NotifyPropertyChanged("SelectedMetricType");
                    NotifyPropertyChanged("SelectedDVHModelType");
                }
            }
        }

        public ObservableCollection<MetricTypes> AllMetricTypes { get; private set; }

        public MetricTypes SelectedMetricType
        {
            get
            {
                if (SelectedDVHMetricSetup != null)
                {
                    return GetMetricType(SelectedDVHMetricSetup);
                }
                else
                {
                    // Return a default type of no metric is selected
                    return MetricTypes.DoseToVolume;
                }
            }
            set
            {
                SetMetricType(SelectedDVHMetricSetup, value);
                NotifyPropertyChanged("SelectedMetricType");
            }
        }

        public ObservableCollection<DVHModelTypes> AllDVHModelTypes { get; private set; }

        public DVHModelTypes SelectedDVHModelType
        {
            get
            {
                if (SelectedDVHMetricSetup != null)
                {
                    return GetDVHModelType(SelectedDVHMetricSetup);
                }
                else
                {
                    // Return a default type of no metric is selected
                    return DVHModelTypes.Standard;
                }
            }
            set
            {
                SetDVHModelTypeBasedOnOld(SelectedDVHMetricSetup, value);
                NotifyPropertyChanged("SelectedDVHModelType");
            }
        }

        public ICommand AddMetricCommand { get; set; }
        public ICommand RemoveMetricCommand { get; set; }

        public MetricEditorViewModel()
        {
            InitializeDVHMetricSetups();
            InitializeAllMetricTypes();
            InitializeAllDVHModelTypes();

            // TODO: Rename these commands to get rid of "Custom"
            // and look into their implementation to eliminate "custom"
            AddMetricCommand = new AddMetricCommand(this);
            RemoveMetricCommand = new RemoveMetricCommand(this);
        }

        private MetricTypes GetMetricType(DVHMetricSetupViewModel dvhMetricSetup)
        {
            MetricViewModel metric = dvhMetricSetup.MetricViewModel;

            if (metric is MeanDoseMetricViewModel)
            {
                return MetricTypes.MeanDose;
            }
            else if (metric is MinDoseMetricViewModel)
            {
                return MetricTypes.MinDose;
            }
            else if (metric is MaxDoseMetricViewModel)
            {
                return MetricTypes.MaxDose;
            }
            else if (metric is StdDevDoseMetricViewModel)
            {
                return MetricTypes.StdDevDose;
            }
            else if (metric is DoseToVolumeMetricViewModel)
            {
                return MetricTypes.DoseToVolume;
            }
            else if (metric is VolumeWithDoseMetricViewModel)
            {
                return MetricTypes.VolumeWithDose;
            }
            else if (metric is ColdVolumeWithDoseMetricViewModel)
            {
                return MetricTypes.ColdVolumeWithDose;
            }
            else if (metric is DoseComplementToVolumeMetricViewModel)
            {
                return MetricTypes.DoseComplementToVolume;
            }
            else if (metric is NTCPMetricViewModel)
            {
                return MetricTypes.NTCP;
            }
            else if (metric is EUDMetricViewModel)
            {
                return MetricTypes.gEUD;
            }
            else if (metric is SMPCMetricViewModel)
            {
                return MetricTypes.SMPC;
            }
            else
            {
                throw new ApplicationException("Unknown metric type.");
            }
        }

        // TODO: Might need to initialize each metric with configured default
        private void SetMetricType(DVHMetricSetupViewModel dvhMetricSetup, MetricTypes metricType)
        {
            dvhMetricSetup.MetricViewModel = MetricViewModel.CreateFromMetric(CreateMetricOfType(metricType));
        }

        private Metric CreateMetricOfType(MetricTypes metricType)
        {
            switch (metricType)
            {
                case MetricTypes.MeanDose:
                    return new MeanDoseMetric();

                case MetricTypes.MinDose:
                    return new MinDoseMetric();

                case MetricTypes.MaxDose:
                    return new MaxDoseMetric();

                case MetricTypes.StdDevDose:
                    return new StdDevDoseMetric();

                case MetricTypes.DoseToVolume:
                    return new DoseToVolumeMetric();

                case MetricTypes.VolumeWithDose:
                    return new VolumeWithDoseMetric();

                case MetricTypes.ColdVolumeWithDose:
                    return new ColdVolumeWithDoseMetric();

                case MetricTypes.DoseComplementToVolume:
                    return new DoseComplementToVolumeMetric();

                case MetricTypes.NTCP:
                    return new NTCPMetric();

                case MetricTypes.gEUD:
                    return new EUDMetric();

                case MetricTypes.SMPC:
                    return new SMPCMetric();

                default:
                    throw new ApplicationException("Unknown metric type.");
            }
        }

        private DVHModelTypes GetDVHModelType(DVHMetricSetupViewModel dvhMetricSetup)
        {
            DVHModelViewModel dvhModel = dvhMetricSetup.DVHModelViewModel;

            if (dvhModel is StandardDVHModelViewModel)
            {
                return DVHModelTypes.Standard;
            }
            else if (dvhModel is LQBioDoseDVHModelViewModel)
            {
                return DVHModelTypes.LQBioDose;
            }
            else if (dvhModel is LQLBioDoseDVHModelViewModel)
            {
                return DVHModelTypes.LQLBioDose;
            }
            else
            {
                throw new ApplicationException("Unknown DVH model type.");
            }
        }

        private void SetDVHModelTypeBasedOnOld
            (DVHMetricSetupViewModel dvhMetricSetup, DVHModelTypes dvhModelType)
        {
            DVHModel oldDVHModel = dvhMetricSetup.DVHMetricSetup.DVHModel;
            DVHModel dvhModel = CreateDVHModelOfTypeBasedOnOld(oldDVHModel, dvhModelType);
            dvhMetricSetup.DVHModelViewModel = DVHModelViewModel.CreateFromDVHModel(dvhModel);
        }

        private DVHModel CreateDVHModelOfTypeBasedOnOld
            (DVHModel oldDVHModel, DVHModelTypes dvhModelType)
        {
            switch (dvhModelType)
            {
                case DVHModelTypes.Standard:
                    return new StandardDVHModel
                    {
                        DoseType = oldDVHModel.DoseType,
                        VolumeType = oldDVHModel.VolumeType
                    };

                // Don't set the dose type of bio-dose models
                // because it should always be absolute
                // (this is enforced in the type itself)

                case DVHModelTypes.LQBioDose:
                    return new LQBioDoseDVHModel
                    {
                        VolumeType = oldDVHModel.VolumeType,
                    };

                case DVHModelTypes.LQLBioDose:
                    return new LQLBioDoseDVHModel
                    {
                        VolumeType = oldDVHModel.VolumeType,
                    };

                default:
                    throw new ApplicationException("Unknown DVH model type.");
            }
        }

        private void InitializeDVHMetricSetups()
        {
            try
            {
                DVHMetricSetups = new ObservableCollection<DVHMetricSetupViewModel>
                    (from dvhMetricSetup in MetricConfig.Load()
                     select new DVHMetricSetupViewModel
                                {
                                    DVHMetricSetup = dvhMetricSetup
                                });
            }
            catch
            {
                // TODO: Inform the user what happened if error reading the file
                DVHMetricSetups = new ObservableCollection<DVHMetricSetupViewModel>();
            }
        }

        private void InitializeAllMetricTypes()
        {
            AllMetricTypes = new ObservableCollection<MetricTypes>
                (new MetricTypes[]
                     {
                         MetricTypes.MeanDose,
                         MetricTypes.MinDose,
                         MetricTypes.MaxDose,
                         MetricTypes.StdDevDose,
                         MetricTypes.DoseToVolume,
                         MetricTypes.VolumeWithDose,
                         MetricTypes.ColdVolumeWithDose,
                         MetricTypes.DoseComplementToVolume,
                         MetricTypes.NTCP,
                         MetricTypes.gEUD,
                         MetricTypes.SMPC
                     });
        }

        private void InitializeAllDVHModelTypes()
        {
            AllDVHModelTypes = new ObservableCollection<DVHModelTypes>
                (new DVHModelTypes[]
                     {
                         DVHModelTypes.Standard,
                         DVHModelTypes.LQBioDose,
                         DVHModelTypes.LQLBioDose
                     });
        }

        public void AddDVHMetricSetup()
        {
            // TODO: Initialize with some kind of configurable default
            DVHMetricSetupViewModel dvhMetricSetupViewModel =
                new DVHMetricSetupViewModel
                {
                    DVHMetricSetup = new DVHMetricSetup
                                         {
                                             DVHModel = new StandardDVHModel(),
                                             Metric = new DoseToVolumeMetric()
                                         }
                };
            DVHMetricSetups.Add(dvhMetricSetupViewModel);
            SelectedDVHMetricSetup = dvhMetricSetupViewModel;

            dvhMetricSetupViewModel.PropertyChanged += OnDVHMetricSetupPropertyChanged;
        }

        private void OnDVHMetricSetupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If an individual DVHMetricSetup property changes,
            // notify that the list DVHMetricSetups changed
            // (this will cause the metric list box to regroup/resort)
            NotifyPropertyChanged("DVHMetricSetups");
        }

        public void SaveMetrics()
        {
            MetricConfig.Save(from dvhMetricSetupViewModel in DVHMetricSetups
                              select dvhMetricSetupViewModel.DVHMetricSetup);
        }
    }
}
