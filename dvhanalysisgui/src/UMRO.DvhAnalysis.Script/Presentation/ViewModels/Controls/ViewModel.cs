using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DVHAnalysis;
using UMRO.DvhAnalysis.AriaDb;
using UMRO.DvhAnalysis.AriaDb.Sql;
using UMRO.DvhAnalysis.Script.Presentation.Commands;
using UMRO.DvhAnalysis.Script.Presentation.Controls;
using UMRO.DvhAnalysis.Script.Presentation.Converters;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    public class ViewModel : BindableBase
    {
        private string _productName;
        public string ProductName
        {
            get { return _productName; }
            set
            {
                _productName = value;
                NotifyPropertyChanged("ProductName");
            }
        }

        private string _productVersion;
        public string ProductVersion
        {
            get { return _productVersion; }
            set
            {
                _productVersion = value;
                NotifyPropertyChanged("ProductVersion");
            }
        }

        private VMS.TPS.Common.Model.API.User _user;
        public VMS.TPS.Common.Model.API.User User
        {
            get { return _user; }
            set
            {
                _user = value;
                NotifyPropertyChanged("User");
            }
        }

        private Patient _patient;
        public Patient Patient
        {
            get { return _patient; }
            set
            {
                _patient = value;
                NotifyPropertyChanged("Patient");
            }
        }

        private Course _course;
        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;
                NotifyPropertyChanged("Course");
            }
        }

        private IList<PlanningItem> _plans;
        public IList<PlanningItem> Plans
        {
            get { return _plans; }
            set
            {
                _plans = value;
                InitializePlanViewModels();
                InitializeStructureViewModels();
            }
        }

        private string _helpUri;
        public string HelpUri
        {
            get { return _helpUri; }
            set
            {
                _helpUri = value;
                NotifyPropertyChanged("HelpUri");
            }
        }

        public IList<PlanViewModel> PlanViewModels { get; private set; }

        public ObservableCollection<PlanViewModel> SelectedPlanViewModels { get; private set; }

        private ObservableCollection<StructureViewModel> _structureViewModels;
        public ObservableCollection<StructureViewModel> StructureViewModels
        {
            get { return _structureViewModels; }
            private set
            {
                _structureViewModels = value;
                NotifyPropertyChanged("StructureViewModels");
            }
        }

        private TemplatesViewModel _templatesViewModel;
        public TemplatesViewModel TemplatesViewModel
        {
            get { return _templatesViewModel; }
            set
            {
                _templatesViewModel = value;
                NotifyPropertyChanged("TemplatesViewModel");
            }
        }

        private ObservableCollection<MetricRowViewModel> _metricRows;
        public ObservableCollection<MetricRowViewModel> MetricRows
        {
            get { return _metricRows; }
            set
            {
                _metricRows = value;
                NotifyPropertyChanged("MetricRows");
            }
        }

        private MetricRowViewModel _selectedMetricRow;
        public MetricRowViewModel SelectedMetricRow
        {
            get { return _selectedMetricRow; }
            set
            {
                _selectedMetricRow = value;
                NotifyPropertyChanged("SelectedMetricRow");
            }
        }

        private ObservableCollection<VolumeRowViewModel> _volumes;
        public ObservableCollection<VolumeRowViewModel> Volumes
        {
            get { return _volumes; }
            set
            {
                _volumes = value;
                NotifyPropertyChanged("Volumes");
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }

        public event EventHandler CalculationWillStart;
        public event EventHandler<ProgressEventArgs> CalcProgressUpdated;
        public event EventHandler CalculationFinished;
        public event EventHandler<CalcNotificationEventArgs> CalculationNotification;

        #region Export DVH options

        private bool _exportDVH;
        public bool ExportDVH
        {
            get { return _exportDVH; }
            set
            {
                _exportDVH = value;
                NotifyPropertyChanged("ExportDVH");
            }
        }

        private bool _exportDVHTypeCumulative;
        public bool ExportDVHTypeCumulative
        {
            get { return _exportDVHTypeCumulative; }
            set
            {
                _exportDVHTypeCumulative = value;
                NotifyPropertyChanged("ExportDVHTypeCumulative");
            }
        }

        private bool _exportDVHDoseAbsolute;
        public bool ExportDVHDoseAbsolute
        {
            get { return _exportDVHDoseAbsolute; }
            set
            {
                _exportDVHDoseAbsolute = value;
                NotifyPropertyChanged("ExportDVHDoseAbsolute");
            }
        }

        private bool _exportDVHVolumeAbsolute;
        public bool ExportDVHVolumeAbsolute
        {
            get { return _exportDVHVolumeAbsolute; }
            set
            {
                _exportDVHVolumeAbsolute = value;
                NotifyPropertyChanged("ExportDVHVolumeAbsolute");
            }
        }

        private bool _exportQCAnalysis;
        public bool ExportQCAnalysis
        {
            get { return _exportQCAnalysis; }
            set
            {
                _exportQCAnalysis = value;
                NotifyPropertyChanged("ExportQCAnalysis");
            }
        }

        #endregion

        #region BioEval report options

        public string PhysicianRequest { get; set; } = BioEvalReportCreator.DefaultRequest;
        public string PhysicsReportNotes { get; set; } = BioEvalReportCreator.DefaultReportNotes;
        public string PhysicianFinalAcknowledgment { get; set; } =
            BioEvalReportCreator.DefaultFinalAcknowledgment;

        #endregion

        public ICommand AddMetricRowCommand { get; private set; }
        public ICommand DeleteMetricRowCommand { get; private set; }

        public ICommand AddAllStructuresCommand { get; private set; }
        public ICommand RemoveAllStructuresCommand { get; private set; }

        /// <summary>
        /// Gets or sets whether the user has been warned
        /// about LQ or LQL models being used on plan sums.
        /// </summary>
        public bool WarnedAboutPlanSum { get; set; }

        public QueryRepository InfoQueryRepository { get; set; }
        public IOncologistRepository OncologistRepository { get; set; }
        public IUserRepository UserRepository { get; set; }

        private MainView mainViewWindow;

        public ViewModel(string userName, MainView mainViewWindown)
        {
            this.mainViewWindow = mainViewWindown;

            PlanViewModels = new ObservableCollection<PlanViewModel>();
            SelectedPlanViewModels = new ObservableCollection<PlanViewModel>();
            StructureViewModels = new ObservableCollection<StructureViewModel>();
            MetricRows = new ObservableCollection<MetricRowViewModel>();
            Volumes = new ObservableCollection<VolumeRowViewModel>();

            TemplatesViewModel = new TemplatesViewModel(userName);
            TemplatesViewModel.Progress += (sender, args) => OnCalcProgressUpdated(args);

            IsBusy = false;

            ExportDVH = false;
            ExportDVHTypeCumulative = true;
            ExportDVHDoseAbsolute = true;
            ExportDVHVolumeAbsolute = false;

            AddMetricRowCommand = new AddMetricRowCommand(this);
            DeleteMetricRowCommand = new DeleteMetricRowCommand(this);

            AddAllStructuresCommand = new RelayCommand
            {
                ExecuteDelegate = AddAllStructures
            };

            RemoveAllStructuresCommand = new RelayCommand
            {
                ExecuteDelegate = RemoveAllStructures
            };
        }

        private void InitializePlanViewModels()
        {
            foreach (PlanningItem plan in Plans)
            {
                PlanViewModel planViewModel = new PlanViewModel { Plan = plan };
                planViewModel.PropertyChanged += OnPlanViewModelPropertyChanged;
                PlanViewModels.Add(planViewModel);
            }
        }

        private void OnPlanViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                UpdateSelectedPlanViewModels();
            }
        }

        private void UpdateSelectedPlanViewModels()
        {
            RemoveUnselectedPlanViewModels();
            AddSelectedPlanViewModels();
        }

        private void RemoveUnselectedPlanViewModels()
        {
            var newSelectedPlans = GetSelectedPlans().ToArray();
            var oldSelectedPlans = SelectedPlanViewModels.ToArray();

            foreach (var plan in oldSelectedPlans)
            {
                if (!newSelectedPlans.Contains(plan))
                {
                    SelectedPlanViewModels.Remove(plan);
                }
            }
        }

        private void AddSelectedPlanViewModels()
        {
            var selectedPlans = GetSelectedPlans();
            foreach (var plan in selectedPlans)
            {
                if (!SelectedPlanViewModels.Contains(plan))
                {
                    SelectedPlanViewModels.Add(plan);
                }
            }
        }

        private void InitializeStructureViewModels()
        {
            StructureViewModels = new ObservableCollection<StructureViewModel>
                (from structureId in GetAllStructureIds()
                 orderby structureId
                 select new StructureViewModel
                            {
                                Id = structureId
                            });
        }

        private IEnumerable<string> GetAllStructureIds()
        {
            // For each plan, generate a list of structure ids,
            // flatten the list, and keep only the unique ids
            return Plans.SelectMany(p => from structure in p.GetStructures()
                                         select structure.Id).Distinct();
        }

        public IEnumerable<PlanViewModel> GetSelectedPlans()
        {
            return from plan in PlanViewModels
                   where plan.IsSelected
                   select plan;
        }

        private void RemoveAllStructures(object parameters)
        {
            while (MetricRows.Count > 0)
            {
                DeleteMetricRow(MetricRows.First());
            }
        }

        private void AddAllStructures(object parameters)
        {
            foreach (StructureViewModel structure in StructureViewModels)
            {
                AddMetricRowForStructure(structure);
            }
        }

        public void AddMetricRowForStructure(StructureViewModel structure)
        {
            MetricRowViewModel metricRowViewModel = new MetricRowViewModel(mainViewWindow.dtaMetrics)
            {
                MainViewModel = this,
                StructureViewModel = structure
            };

            metricRowViewModel.CalculationNotification +=
                (sender, args) => OnCalculationNotification(args);

            MetricRows.Add(metricRowViewModel);
            SelectedMetricRow = metricRowViewModel;

            UpdateVolumes();
        }

        public void DeleteMetricRow()
        {
            DeleteMetricRow(SelectedMetricRow);
        }

        public void DeleteMetricRow(MetricRowViewModel metricRow)
        {
            MetricRows.Remove(metricRow);
            SelectedMetricRow = null;
            UpdateVolumes();
        }

        public void UpdateVolumes()
        {
            Volumes = new ObservableCollection<VolumeRowViewModel>
                (from structure in GetActiveStructures()
                 select VolumeRowViewModel.CreateForPlans(structure, Plans));
        }

        public IEnumerable<StructureViewModel> GetActiveStructures()
        {
            return (from metricRow in MetricRows
                    select metricRow.StructureViewModel).Distinct();
        }

        public MetricRowViewModel GetFirstBioDoseMetricRowForStructure(StructureViewModel structure)
        {
            return (from metricRow in MetricRows
                    where metricRow.StructureViewModel == structure &&
                          metricRow.SelectedDVHMetricSetupViewModel != null &&
                          metricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel is BioDoseDVHModel
                    select metricRow).FirstOrDefault();
        }

        public MetricRowViewModel GetFirstLQBioDoseMetricRowForStructure(StructureViewModel structure)
        {
            return (from metricRow in MetricRows
                    where metricRow.StructureViewModel == structure &&
                          metricRow.SelectedDVHMetricSetupViewModel != null &&
                          metricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel is LQBioDoseDVHModel
                    select metricRow).FirstOrDefault();
        }

        public MetricRowViewModel GetFirstLQLBioDoseMetricRowForStructure(StructureViewModel structure)
        {
            return (from metricRow in MetricRows
                    where metricRow.StructureViewModel == structure &&
                          metricRow.SelectedDVHMetricSetupViewModel != null &&
                          metricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel is LQLBioDoseDVHModel
                    select metricRow).FirstOrDefault();
        }

        public void ApplyTemplate(TemplateViewModel templateViewModel, ICollection<StructureMatchViewModel> structureMatches)
        {
            // This event is being listened by the window,
            // which then creates the calc progress view model and window,
            // and displays it
            OnCalculationWillStart();

            TemplatesViewModel.ApplyTemplate(templateViewModel, this, structureMatches);

            OnCalculationFinished();

            // Load a new set of templates, so that any changes
            // to the metrics don't affect the original template
            TemplatesViewModel = new TemplatesViewModel(User.Id);
            TemplatesViewModel.Progress += (sender, args) => OnCalcProgressUpdated(args);
        }

        public void ShowBioEvalReport()
        {
            try
            {
                IsBusy = true;
                Process.Start(CreateBioEvalReport());
            }
            catch (Exception ex)
            {
                NotifyUserMessaged("Error generating document",
                    $"There was a problem generating the document. Error message: {ex.Message}",
                    UserMessageType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UploadBioEvalReportAsync()
        {
            try
            {
                IsBusy = true;
                var ariaDocClient = new AriaDocumentClient(User.Id, Patient.Id);
                var reportPath = CreateBioEvalReport();
                await ariaDocClient.InsertDocumentAsync(reportPath, "PDF", "Special Medical Physics Consultation");
                NotifyUserMessaged("Success uploading document",
                    $"The document was successfully uploaded.", UserMessageType.Information);
            }
            catch (Exception ex)
            {
                NotifyUserMessaged("Error uploading document",
                    $"There was a problem uploading the document. Error message: {ex.Message}",
                    UserMessageType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public string CreateBioEvalReport()
        {
            var displayDvhs = new DisplayDvhs(this)
            {
                DoseType = DoseValuePresentation.Absolute,
                VolumeType = VolumePresentation.Relative,
                DvhType = DisplayDvhType.Cumulative
            };

            var planNames = PlanViewModels.Where(p => p.IsSelected).Select(p => p.Name).ToList();

            BioEvalReportCreator reportCreator = new BioEvalReportCreator
            {
                PatientName = Patient.FirstName + " " + Patient.LastName,
                PatientId = Patient.Id,
                DateOfBirth = Patient.DateOfBirth,
                Sex = Patient.Sex,
                PhysicianName = GetPhysicianName(Patient.PrimaryOncologistId),
                RequestText = PhysicianRequest,
                PlanNames = planNames,
                Data = GetBioEvalData(planNames),
                Dvhs = displayDvhs.GetDvhs().ToList(),
                ReportNotes = PhysicsReportNotes,
                PhysicistName = GetUserName(User.Id),
                FinalAcknowledgment = PhysicianFinalAcknowledgment
            };

            string reportPath = Path.GetTempFileName() + ".pdf";

            reportCreator.CreateReport(reportPath);

            return reportPath;
        }

        private IList<BioEvalMetricRow> GetBioEvalData(IEnumerable<string> planNames)
        {
            var converter = new MetricResultConverter();
            var rv = MetricRows.OrderBy(mr => mr.StructureViewModel.Id)  // Warning, didn't enforce metrics orders here. May cause inconsistency
                             .Select(mr => new BioEvalMetricRow
                {
                    Roi = mr.StructureViewModel.Id,
                    Metric = mr.SelectedDVHMetricSetupViewModel.Name,
                    Values = mr.MetricResults
                        .Where(r => planNames.Contains(r.EclipseData.Plan.Id))
                        .Select(r => (string)converter.Convert(r,
                            typeof(string), null, CultureInfo.CurrentCulture)).ToArray(),
                    Paramaters = GetMetricParameters(mr.SelectedDVHMetricSetupViewModel),
                }).ToList();

            return rv;
        }

        private string GetMetricParameters(DVHMetricSetupViewModel dvhMetricSetupVm)
        {
            string parameters = string.Empty;

            // Add parameters for NTCP and gEUD metrics
            if (dvhMetricSetupVm.MetricViewModel is NTCPMetricViewModel)
            {
                NTCPMetricViewModel metric =
                    dvhMetricSetupVm.MetricViewModel as NTCPMetricViewModel;
                parameters += $"n = {metric.LKBn}, m = {metric.LKBm}, TD50 = {metric.LKBD50}";
            }
            else if (dvhMetricSetupVm.MetricViewModel is EUDMetricViewModel)
            {
                EUDMetricViewModel metric =
                    dvhMetricSetupVm.MetricViewModel as EUDMetricViewModel;
                parameters += $"a = {metric.a}";
            }

            // Add biodose parameters
            if (dvhMetricSetupVm.DVHModelViewModel is LQBioDoseDVHModelViewModel)
            {
                LQBioDoseDVHModelViewModel dvhModel =
                    dvhMetricSetupVm.DVHModelViewModel as LQBioDoseDVHModelViewModel;

                if (parameters != string.Empty)
                {
                    parameters += ", ";
                }

                parameters += $"LQ: \u03B1/\u03B2 = {dvhModel.AlphaBeta}";
            }
            else if (dvhMetricSetupVm.DVHModelViewModel is LQLBioDoseDVHModelViewModel)
            {
                LQLBioDoseDVHModelViewModel dvhModel =
                    dvhMetricSetupVm.DVHModelViewModel as LQLBioDoseDVHModelViewModel;

                if (parameters != string.Empty)
                {
                    parameters += ", ";
                }

                parameters += $"LQL: \u03B1/\u03B2 = {dvhModel.AlphaBeta}, DT = {dvhModel.DT}";
            }

            return parameters;
        }

        private string GetPhysicianName(string oncologistId)
        {
            try
            {
                var oncologist = OncologistRepository.FindById(oncologistId);
                return oncologist.FullName;
            }
            catch
            {
                return "N/A";
            }
        }

        private string GetUserName(string userId)
        {
            try
            {
                var user = UserRepository.FindById(userId);
                return user.Name;
            }
            catch
            {
                return "N/A";
            }
        }

        protected virtual void OnCalculationWillStart()
        {
            var handler = CalculationWillStart;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnCalcProgressUpdated(ProgressEventArgs e)
        {
            var handler = CalcProgressUpdated;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnCalculationFinished()
        {
            var handler = CalculationFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnCalculationNotification(CalcNotificationEventArgs e)
        {
            var handler = CalculationNotification;
            if (handler != null) handler(this, e);
        }
    }
}
