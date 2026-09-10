using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using DVHAnalysis;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using UMRO.DvhAnalysis.Script.Presentation.Controls;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class MetricRowViewModel : BindableBase
    {
        private static readonly ILogger _log = AppLog.GetLogger(nameof(MetricRowViewModel));

        private ViewModel _mainViewModel;
        public ViewModel MainViewModel
        {
            get { return _mainViewModel; }
            set
            {
                _mainViewModel = value;
                NotifyPropertyChanged("MainViewModel");
            }
        }

        private StructureViewModel _structureViewModel;
        public StructureViewModel StructureViewModel
        {
            get { return _structureViewModel; }
            set
            {
                _structureViewModel = value;
                NotifyPropertyChanged("StructureViewModel");
            }
        }

        private ObservableCollection<DVHMetricSetupViewModel> _allDVHMetricSetups;
        public ObservableCollection<DVHMetricSetupViewModel> AllDVHMetricSetups
        {
            get { return _allDVHMetricSetups; }
            set
            {
                _allDVHMetricSetups = value;
                NotifyPropertyChanged("AllDVHMetricSetups");
            }
        }

        private DVHMetricSetupViewModel _selectedDVHMetricSetupViewModel;
        public DVHMetricSetupViewModel SelectedDVHMetricSetupViewModel
        {
            get { return _selectedDVHMetricSetupViewModel; }
            set
            {
                _selectedDVHMetricSetupViewModel = value;
                NotifyPropertyChanged("SelectedDVHMetricSetupViewModel");
            }
        }

        private ObservableCollection<MetricResult> _metricResults;
        public ObservableCollection<MetricResult> MetricResults
        {
            get { return _metricResults; }
            set
            {
                _metricResults = value;
                NotifyPropertyChanged("MetricResults");
            }
        }

        public event EventHandler<CalcNotificationEventArgs> CalculationNotification;

        public readonly DataGrid main_wind_dg;

        public MetricRowViewModel(DataGrid main_wind_dg)
        {
            this.main_wind_dg = main_wind_dg;

            try
            {
                AllDVHMetricSetups = new ObservableCollection<DVHMetricSetupViewModel>
                    (from metricSetup in MetricConfig.Load()
                     select new DVHMetricSetupViewModel
                                {
                                    DVHMetricSetup = metricSetup
                                });
            }
            catch
            {
                AllDVHMetricSetups = new ObservableCollection<DVHMetricSetupViewModel>();
            }

            MetricResults = new ObservableCollection<MetricResult>();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedDVHMetricSetupViewModel":
                    OnSelectedDVHMetricSetupViewModelChanged();
                    break;
            }
        }

        private void OnSelectedDVHMetricSetupViewModelChanged()
        {
            CalculateForAllPlans();
            ListenToMetricPropertyChanges();
        }

        public void CalculateForAllPlans()
        {
            MainViewModel.IsBusy = true;

            MetricResults.Clear();

            foreach (PlanViewModel plan in MainViewModel.PlanViewModels)   // this order in MainViewModel.PlanViewModels keep the order of dtaMetrics.Columns and MetricResults correct.
                                                                           // See InitializeMetricDataGrid() in MainView.xaml.cs --> Binding = new Binding("MetricResults[" + i + "]")
                                                                           // The order of plans in MainViewModel.PlanViewModels Should NEVER change, otherwise calculated MetricResults[i] won't match the initial Binding
            {
                try
                {
                    MetricResult metricResult = CalculateForPlan(plan);
                    MetricResults.Add(metricResult);

                    if (SelectedDVHModelIsBioDose() && plan.Plan is PlanSum)
                    {
                        // This notification must happen after the metric result is added;
                        // otherwise, metrics may appear out of order
                        if (!MainViewModel.WarnedAboutPlanSum)
                        {
                            string message = "Warning in plan " + plan.Name + ": " +
                                             "LQ and LQ-L metrics for plan sums assume the plans " +
                                             "are to be delivered sequentially. If plans are to be " +
                                             "delivered concurrently, metrics may be underestimated. " +
                                             "Please contact physics for assistance.";
                            OnCalculationNotification(new CalcNotificationEventArgs(
                                message, CalcNotificationType.Warning));
                            MainViewModel.WarnedAboutPlanSum = true;
                        }
                    }

                    if (SelectedDVHModelIsBioDose() && IsPlanSumAndHasDifferentDataSets(plan.Plan))
                    {
                        // This notification must happen after the metric result is added;
                        // otherwise, metrics may appear out of order
                        string message = "Warning in plan sum " + plan.Name + ": " +
                                         "This plan sum has plans with different data sets. " +
                                         "In order to properly sum doses, any doses outside " +
                                         "the data grid of either plan will be set to 0.0 Gy.";
                        OnCalculationNotification(new CalcNotificationEventArgs(
                            message, CalcNotificationType.Warning));

                        if (HasMultipleRegistrations((PlanSum) plan.Plan))
                        {
                            message = "Warning in plan sum " + plan.Name + ": " +
                                      "This plan sum has multiple registrations " +
                                      "for a pair of data sets. Only one of them was used.";
                            OnCalculationNotification(new CalcNotificationEventArgs(
                                message, CalcNotificationType.Warning));
                        }
                    }
                }
                catch (StructureDoesNotExistInPlanException)
                {
                    AddInvalidMetricResult(plan);
                    // Requirement: don't show a message if structure doesn't exist in plan
                }
                catch (Exception e)
                {
                    AddInvalidMetricResult(plan);

                    _log.Error($"Metric calculation failed for structure '{StructureViewModel.Id}', " +
                               $"metric '{SelectedDVHMetricSetupViewModel?.Name}', plan '{plan.Name}'", e);

                    string errorMessage = "Error in plan " + plan.Name + ": " + e.Message;
                    OnCalculationNotification(new CalcNotificationEventArgs(
                        errorMessage, CalcNotificationType.Error));
                }
            }


            if (SelectedDVHMetricSetupViewModel.DVHMetricSetup.Metric is SMPCMetric)
            {
                var planVMs = MainViewModel.PlanViewModels.Where(t => t.IsSelected).ToList();   // Only visible column here.

                var planVMs_psum = planVMs.Where(t => true || t.IsPlanSum).ToList();  // SMPC Metric only apply to plansums?? Not using this restriction yet.

                Dictionary<string,double> manual_input = new Dictionary<string, double>();

                var ordered_cols = main_wind_dg.Columns.OrderBy(t => t.DisplayIndex).ToList();

                foreach (var col1 in ordered_cols)
                {
                    string pn = col1.Header.ToString();

                    if (!planVMs_psum.Any(t => t.Name == pn)) continue;

                    manual_input.Add(pn, double.NaN);
                }


                var input_window = new SMPCMetricInput(manual_input, StructureViewModel.Id, SelectedDVHMetricSetupViewModel.DVHMetricSetup.Name);

                input_window.Topmost = true;

                MainViewModel.IsBusy = false;

                if (input_window.ShowDialog() == true)
                {
                    foreach (var mr in MetricResults)
                    {
                        string planName = mr.EclipseData.Plan.Id;

                        if (manual_input.ContainsKey(planName))
                        {
                            mr.Value = manual_input[planName];

                            if(double.IsNaN(manual_input[planName]))
                            {
                                mr.Unit = MetricUnit.Invalid;
                            }
                        }
                    }
                }
             
                // Force the DataGrid to refresh:
                var view = CollectionViewSource.GetDefaultView(MainViewModel.MetricRows);

                view.Refresh();

            }

            MainViewModel.IsBusy = false;
        }

        private void AddInvalidMetricResult(PlanViewModel plan)
        {
            var invalidMetricResult = new MetricResult
            {
                // This is needed so that the metric result (even if invalid)
                // is associated with a specific plan (used for bio-eval report)
                EclipseData = new EclipseData
                {
                    Patient = MainViewModel.Patient,
                    Course = MainViewModel.Course,
                    Plan = plan.Plan,
                    Structure = StructureViewModel.GetPlanStructure(plan.Plan)
                },
                Unit = MetricUnit.Invalid
            };

            MetricResults.Add(invalidMetricResult);
        }

        private bool IsPlanSumAndHasDifferentDataSets(PlanningItem planningItem)
        {
            return (planningItem is PlanSum) && HasDifferentDataSets((PlanSum)planningItem);
        }

        private bool HasDifferentDataSets(PlanSum planSum)
        {
            if (planSum.PlanSetups == null)
            {
                return false;
            }

            var plans = planSum.PlanSetups.ToArray();
            return plans.Any(p => p.StructureSet.UID != plans.First().StructureSet.UID);
        }

        private bool HasMultipleRegistrations(PlanSum planSum)
        {
            if (planSum.PlanSetups == null)
            {
                return false;
            }

            var planSumFOR = planSum.StructureSet.Image.FOR;
            var planFORs = planSum.PlanSetups.Select(p => p.StructureSet.Image.FOR).ToArray();
            var regs = planSum.Course.Patient.Registrations.ToArray();

            // Get all registrations (in either direction) that deal with
            // the plan sum data set and any data set of its plans.
            var validRegs = regs.Where(r =>
                (r.SourceFOR == planSumFOR && planFORs.Contains(r.RegisteredFOR)) ||
                (planFORs.Contains(r.SourceFOR) && r.RegisteredFOR == planSumFOR));

            return validRegs.Count() > 1;
        }

        public MetricResult CalculateForPlan(PlanViewModel plan)
        {
            EclipseData eclipseData = new EclipseData
            {
                Patient = MainViewModel.Patient,
                Course = MainViewModel.Course,
                Plan = plan.Plan,
                Structure = StructureViewModel.GetPlanStructure(plan.Plan)
            };

            if (eclipseData.Structure == null)
            {
                throw new StructureDoesNotExistInPlanException(
                    "The structure " + StructureViewModel.Id + " does not exist " +
                    "in plan " + plan.Name + ".");
            }

            return SelectedDVHMetricSetupViewModel.Calculate(eclipseData);
        }

        private bool SelectedDVHModelIsBioDose()
        {
            DVHMetricSetupViewModel metric = SelectedDVHMetricSetupViewModel;

            if (metric != null)
            {
                var dvhMetricSetup = metric.DVHMetricSetup;

                if (dvhMetricSetup != null)
                {
                    return dvhMetricSetup.DVHModel is BioDoseDVHModel;
                }
            }

            return false;
        }

        public void ListenToMetricPropertyChanges()
        {
            // Any changes to these should clear metric results
            SelectedDVHMetricSetupViewModel.PropertyChanged += OnMetricPropertyChanged;
            SelectedDVHMetricSetupViewModel.DVHModelViewModel.PropertyChanged += OnMetricPropertyChanged;
            SelectedDVHMetricSetupViewModel.MetricViewModel.PropertyChanged += OnMetricPropertyChanged;
        }

        private void OnMetricPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DVHModelViewModel":
                    // If the DVH model itself is changed, re-listen to its property changes;
                    // don't have to re-listen to metric changes because they cannot be changed
                    SelectedDVHMetricSetupViewModel.DVHModelViewModel.PropertyChanged +=
                        OnMetricPropertyChanged;

                    // If alpha/beta is uninitialized (= 0), set it to 2.5 (normal) or 10 (target)
                    SetAlphaBetaForSelectedDVHModel();
                    break;
            }

            MetricResults.Clear();
        }

        private void SetAlphaBetaForSelectedDVHModel()
        {
            // Use the first plan to determine if the structure belong to it is a target
            PlanViewModel planVM = MainViewModel.PlanViewModels.FirstOrDefault();

            if (planVM != null)
            {
                if (StructureViewModel.IsTarget(planVM.Plan))
                {
                    SetAlphaBetaIfZeroForSelectedDVHModel(10.0);
                }
                else
                {
                    SetAlphaBetaIfZeroForSelectedDVHModel(2.5);
                }
            }
        }

        // Change the alpha/beta only if it's zero (i.e., uninitialized)
        private void SetAlphaBetaIfZeroForSelectedDVHModel(double alphaBeta)
        {
            DVHModelViewModel dvhModelVM = SelectedDVHMetricSetupViewModel.DVHModelViewModel;

            if (dvhModelVM is LQBioDoseDVHModelViewModel)
            {
                LQBioDoseDVHModelViewModel lqModel = dvhModelVM as LQBioDoseDVHModelViewModel;

                if (lqModel.AlphaBeta == 0.0)
                {
                    lqModel.AlphaBeta = alphaBeta;
                }
            }
            else if (dvhModelVM is LQLBioDoseDVHModelViewModel)
            {
                LQLBioDoseDVHModelViewModel lqlModel = dvhModelVM as LQLBioDoseDVHModelViewModel;

                if (lqlModel.AlphaBeta == 0.0)
                {
                    lqlModel.AlphaBeta = alphaBeta;
                }
            }
        }

        protected virtual void OnCalculationNotification(CalcNotificationEventArgs e)
        {
            var handler = CalculationNotification;
            if (handler != null) handler(this, e);
        }
    }
}
