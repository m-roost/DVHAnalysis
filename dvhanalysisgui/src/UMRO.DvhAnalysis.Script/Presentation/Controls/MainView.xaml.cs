using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DVHAnalysis;
using UMRO.DvhAnalysis.AriaDb;
using UMRO.DvhAnalysis.AriaDb.Sql;
using UMRO.DvhAnalysis.Script.Presentation.Converters;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private ViewModel ViewModel;

        public static readonly DependencyProperty MetricListProperty =
            DependencyProperty.Register("MetricList", typeof(List<string>), typeof(MainView));
        public List<string> MetricList
        {
            get { return (List<string>)GetValue(MetricListProperty); }
            set { SetValue(MetricListProperty, value); }
        }

        public StructureSet StructureSet { get; set; }
        public PlanningItem Plan { get; set; }
        public IList<PlanningItem> Plans { get; set; }
        public Patient Patient { get; set; }
        public Course Course { get; set; }
        public VMS.TPS.Common.Model.API.User User { get; set; }


        public QueryRepository InfoQueryRepository { get; set; }

        public IOncologistRepository OncologistRepository { get; set; }
        public IUserRepository UserRepository { get; set; }

        public CalculatingWindow CalcWindow { get; set; }

        #region Initialization

        public MainView()
        {
            InitializeComponent();
            InitializeMetricList();
            BackgroundRunner.Dispatcher = Dispatcher;
        }

        private void InitializeMetricList()
        {
            MetricList = new List<string>();
            MetricList.AddRange(GetMetricNames());
        }

        private IEnumerable<string> GetMetricNames()
        {
            try
            {
                return from DVHMetricSetup metric in MetricConfig.Load()
                       select metric.Name;
            }
            catch
            {
                return new List<string>();
            }
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            InitializeViewModel();
            var tb = TitleBar;
        }

        private void InitializeViewModel()
        {
            ViewModel = new ViewModel(User.Id, this);
            DataContext = ViewModel;

            ViewModel.ProductName = "DVHAnalysis";
            ViewModel.ProductVersion = GetProductVersion();
            ViewModel.User = User;
            ViewModel.Patient = Patient;
            ViewModel.Course = Course;
            ViewModel.Plans = Plans;
            ViewModel.HelpUri = GetHelpUri();
            ViewModel.InfoQueryRepository = InfoQueryRepository;
            ViewModel.OncologistRepository = OncologistRepository;
            ViewModel.UserRepository = UserRepository;

            ICollectionView cvs = CollectionViewSource.GetDefaultView(ViewModel.MetricRows);
            if (cvs != null && cvs.CanGroup)
            {
                cvs.GroupDescriptions.Clear();
                cvs.GroupDescriptions.Add(new PropertyGroupDescription("StructureViewModel.Id"));
            }

            ViewModel.SelectedPlanViewModels.CollectionChanged += OnSelectedPlansChanged;
            ViewModel.MetricRows.CollectionChanged += OnMetricRowsChanged;

            // Be notified when the view model starts or finishes calculating metrics
            ViewModel.CalculationWillStart += OnCalculationWillStart;
            ViewModel.CalculationFinished += OnCalculationFinished;

            // Handle any calculation notifications (by default, as a message box)
            ViewModel.CalculationNotification += OnCalculationNotification;

            // Change the cursor depending on the busy status
            ViewModel.PropertyChanged += OnIsBusyChanged;

            InitializeDataGrids();
        }

        private void OnCalculationWillStart(object sender, EventArgs eventArgs)
        {
            // Run window in a new thread to allow calculation to start (in ViewModel)
            // and to let the progress bar animations occur smoothly;
            // solution obtained from http://reedcopsey.com/2011/11/28/launching-a-wpf-window-in-a-separate-thread-part-1
            Thread thread = new Thread(() =>
            {
                // Create and set a synchronization context
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(
                        Dispatcher.CurrentDispatcher));

                CalcWindow = new CalculatingWindow
                {
                    ViewModel = new CalcProgressViewModel(ViewModel)
                };

                // When the window is closed, shut down the dispatcher
                CalcWindow.Closed += (o, args) =>
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(
                        DispatcherPriority.Background);

                CalcWindow.ShowDialog();

                // Start the dispatcher
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void OnCalculationFinished(object sender, EventArgs e)
        {
            CalcWindow.CloseIfNoNotifications();
        }

        private string GetProductVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private string GetHelpUri()
        {
            return AssemblySettings.DocumentPath;
        }

        private void InitializeDataGrids()
        {
            InitializeMetricDataGrid();
            InitializeVolumeDataGrid();
        }

        private void InitializeMetricDataGrid()
        {
            IList<PlanViewModel> plans = ViewModel.PlanViewModels;
            for (int i = 0; i < plans.Count; i++)
            {
                DataGridTextColumn metricCol = new DataGridTextColumn
                {
                    Header = plans[i].Name,
                    Binding = new Binding("MetricResults[" + i + "]")
                                  {
                                      Converter = new MetricResultConverter()
                                  }
                };

                dtaMetrics.Columns.Add(metricCol);
            }
        }

        private void InitializeVolumeDataGrid()
        {
            IList<PlanViewModel> plans = ViewModel.PlanViewModels;
            for (int i = 0; i < plans.Count; i++)
            {
                DataGridTextColumn volCol = new DataGridTextColumn
                {
                    Header = plans[i].Name,
                    Binding = new Binding("Volumes[" + i + "]")
                                  {
                                      StringFormat = "{0:F2}"
                                  }
                };

                dtaVolumes.Columns.Add(volCol);
            }
        }

        private void OnSelectedPlansChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDataGridsColumnVisibility();
        }

        private void UpdateDataGridsColumnVisibility()
        {
            UpdateDataGridColumnVisibility(dtaMetrics);
            UpdateDataGridColumnVisibility(dtaVolumes);
        }

        private void UpdateDataGridColumnVisibility(DataGrid dataGrid)
        {
            foreach (DataGridColumn column in GetPlanColumns(dataGrid))
            {
                column.Visibility = GetColumnPlan(column).IsSelected ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        private IList<DataGridColumn> GetPlanColumns(DataGrid dataGrid)
        {
            return (from column in dataGrid.Columns
                    where ColumnIsPlanColumn(column)
                    select column).ToList();
        }

        private bool ColumnIsPlanColumn(DataGridColumn column)
        {
            return GetColumnPlan(column) != null;
        }

        private PlanViewModel GetColumnPlan(DataGridColumn column)
        {
            return (from plan in ViewModel.PlanViewModels
                    where plan.Name == column.Header.ToString()
                    select plan).FirstOrDefault();
        }

        private void OnMetricRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            foreach (object o in e.NewItems)
            {
                MetricRowViewModel metricRow = o as MetricRowViewModel;
                UpdateSortingAndGrouping(metricRow.AllDVHMetricSetups);
            }
        }

        // TODO: When MetricsViewModel is created and shared, this should go there
        private void UpdateSortingAndGrouping(object source)
        {
            ICollectionView cvs = CollectionViewSource.GetDefaultView(source);
            if (cvs != null && cvs.CanGroup && cvs.CanSort)
            {
                cvs.GroupDescriptions.Clear();
                cvs.GroupDescriptions.Add(new PropertyGroupDescription("MetricViewModel.TypeName"));

                cvs.SortDescriptions.Clear();
                cvs.SortDescriptions.Add
                    (new SortDescription("MetricViewModel.TypeName", ListSortDirection.Ascending));
                cvs.SortDescriptions.Add
                    (new SortDescription("Name", ListSortDirection.Ascending));
            }
        }

        #endregion // Initialization

        #region Column reordering

        private void dtaMetrics_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            MatchColumnOrder(dtaMetrics, dtaVolumes);
        }

        private void dtaVolumes_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            MatchColumnOrder(dtaVolumes, dtaMetrics);
        }

        private void MatchColumnOrder(DataGrid source, DataGrid target)
        {
            var targetColumns = target.Columns.Where(t => !t.IsFrozen);
            foreach (DataGridColumn targetColumn in targetColumns)
            {
                DataGridColumn sourceColumn = source.Columns.FirstOrDefault
                    (s => s.Header.ToString() == targetColumn.Header.ToString());

                int indexDiff = target.FrozenColumnCount - source.FrozenColumnCount;
                targetColumn.DisplayIndex = sourceColumn.DisplayIndex + indexDiff;
            }
        }

        #endregion // Column reordering

        #region DVH

        private void btnDVH_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            UMRO.Utils.DVHViewer.DVHViewer viewer = new UMRO.Utils.DVHViewer.DVHViewer();
            if (Plans.Count == 1)
            {
                if (Plan is PlanSetup)
                    viewer.PrescribedDose = (Plan as PlanSetup).TotalDose.Dose;
                else
                    viewer.PrescribedDose = (Plan as PlanSum).PlanSetups.First().TotalDose.Dose;
            }

            DvhColors colors = new DvhColors();
            foreach (StructureViewModel vmStructure in ViewModel.GetActiveStructures())
            {
                foreach (PlanningItem plan in Plans)
                {
                    Color color = colors.Next();

                    EclipseData eclipseData = new EclipseData
                    {
                        Patient = ViewModel.Patient,
                        Course = ViewModel.Course,
                        Plan = plan,
                        Structure = vmStructure.GetPlanStructure(plan)
                    };

                    DVHModel dvhModel = new StandardDVHModel
                    {
                        DoseType = DoseValuePresentation.Absolute,
                        VolumeType = VolumePresentation.AbsoluteCm3
                    };

                    DVH dvh;
                    try
                    {
                        dvh = dvhModel.Calculate(eclipseData);
                    }
                    catch
                    {
                        dvh = null;
                    }

                    if (dvh != null)
                    {
                        Point[] dvhPoints = new Point[dvh.CurveData.Count()];
                        for (int i = 0; i < dvh.CurveData.Count(); i++)
                        {
                            dvhPoints[i].X = dvh.CurveData[i].Dose;
                            dvhPoints[i].Y = dvh.CurveData[i].Volume;
                        }

                        viewer.AddCumulativeDVH(vmStructure.Id + "-" + plan.Id, color,
                            dvhPoints, "Gy", dvh.CurveData[0].Volume);
                    }

                    //Check if we have a LQ based bio corrected DVH
                    MetricRowViewModel metricRow = ViewModel.GetFirstLQBioDoseMetricRowForStructure(vmStructure);

                    if (metricRow != null)
                    {
                        LQBioDoseDVHModel bioDVHModel =
                            metricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel as LQBioDoseDVHModel;

                        dvhModel = new LQBioDoseDVHModel
                        {
                            VolumeType = VolumePresentation.AbsoluteCm3,
                            AlphaBeta = bioDVHModel.AlphaBeta,
                        };

                        DVH bioDvh;
                        try
                        {
                            bioDvh = dvhModel.Calculate(eclipseData);
                        }
                        catch
                        {
                            bioDvh = null;
                        }

                        if (bioDvh != null)
                        {
                            Point[] dvhPoints = new Point[bioDvh.CurveData.Count()];
                            for (int i = 0; i < bioDvh.CurveData.Count(); i++)
                            {
                                dvhPoints[i].X = bioDvh.CurveData[i].Dose;
                                dvhPoints[i].Y = bioDvh.CurveData[i].Volume;
                            }

                            viewer.AddCumulativeDVH(vmStructure.Id + "-" + plan.Id + "(EQD2-LQ)",
                                color, dvhPoints, "Gy", bioDvh.CurveData[0].Volume);
                        }

                    }

                    //Check if we have a LQL based bio corrected DVH
                    metricRow = ViewModel.GetFirstLQLBioDoseMetricRowForStructure(vmStructure);

                    if (metricRow != null)
                    {
                        LQLBioDoseDVHModel bioDVHModel =
                            metricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel as LQLBioDoseDVHModel;

                        dvhModel = new LQLBioDoseDVHModel
                        {
                            VolumeType = VolumePresentation.AbsoluteCm3,
                            AlphaBeta = bioDVHModel.AlphaBeta,
                            DT = bioDVHModel.DT,
                        };

                        DVH bioDvh;
                        try
                        {
                            bioDvh = dvhModel.Calculate(eclipseData);
                        }
                        catch
                        {
                            bioDvh = null;
                        }

                        if (bioDvh != null)
                        {
                            Point[] dvhPoints = new Point[bioDvh.CurveData.Count()];
                            for (int i = 0; i < bioDvh.CurveData.Count(); i++)
                            {
                                dvhPoints[i].X = bioDvh.CurveData[i].Dose;
                                dvhPoints[i].Y = bioDvh.CurveData[i].Volume;
                            }

                            viewer.AddCumulativeDVH(vmStructure.Id + "-" + plan.Id + "(EQD2-LQL)",
                                color, dvhPoints, "Gy", bioDvh.CurveData[0].Volume);
                        }

                    }
                }
            }

            Cursor = Cursors.Arrow;

            Window window = new Window();
            window.Content = viewer;
            window.Show();
        }

        #endregion // DVH


        #region Save to Database

        private void btnSaveToDB_Click(object sender, RoutedEventArgs e)
        {
            (new SaveToDBWindow(ViewModel, dtaMetrics)).ShowDialog();


        }


        #endregion // Save to Database



        #region Excel export

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportOptions exportOptions = new ExportOptions
            {
                ExportDVH = ViewModel.ExportDVH,
                ExportDVHTypeCumulative = ViewModel.ExportDVHTypeCumulative,
                ExportDVHDoseAbsolute = ViewModel.ExportDVHDoseAbsolute,
                ExportDVHVolumeAbsolute = ViewModel.ExportDVHVolumeAbsolute,
                ExportQCAnalysis = ViewModel.ExportQCAnalysis
            };

            if (exportOptions.ShowDialog() == true)
            {
                ViewModel.ExportDVH = exportOptions.ExportDVH;
                ViewModel.ExportDVHTypeCumulative = exportOptions.ExportDVHTypeCumulative;
                ViewModel.ExportDVHDoseAbsolute = exportOptions.ExportDVHDoseAbsolute;
                ViewModel.ExportDVHVolumeAbsolute = exportOptions.ExportDVHVolumeAbsolute;
                ViewModel.ExportQCAnalysis = exportOptions.ExportQCAnalysis;

                List<string> displayHeaders = new List<string>();
                List<int> planFractions = new List<int>();

                for (int i = 0; i < dtaMetrics.Columns.Count; i++)
                {
                    DataGridColumn col = dtaMetrics.Columns.FirstOrDefault(s => s.DisplayIndex == i);

                    if (col.Visibility != System.Windows.Visibility.Visible)
                    {
                        continue;
                    }

                    if (col.Header.ToString() != "Metric")
                    {
                        string planId = col.Header.ToString();
                        displayHeaders.Add(planId);
                        //get plan number of fractions
                        int nFractions = 0;
                        PlanningItem pItem = Plans.FirstOrDefault(s => s.Id == planId);
                        if (pItem is PlanSetup)
                        {
                            nFractions = (int)(pItem as PlanSetup).NumberOfFractions;
                        }
                        planFractions.Add(nFractions);
                    }
                }

                ExcelDocumentCreator excelCreator = new ExcelDocumentCreator()
                {
                    ViewModel = ViewModel,
                    ExportDVH = ViewModel.ExportDVH,
                    ExportDVHTypeCumulative = ViewModel.ExportDVHTypeCumulative,
                    ExportDVHDoseAbsolute = ViewModel.ExportDVHDoseAbsolute,
                    ExportDVHVolumeAbsolute = ViewModel.ExportDVHVolumeAbsolute,
                    ExportQCAnalysis = ViewModel.ExportQCAnalysis
                };

                Cursor = Cursors.Wait;

                try
                {
                    excelCreator.CreateDocument(displayHeaders, planFractions,
                        ViewModel.StructureViewModels.ToList(), Plans.ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to create the Excel file. Error: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Cursor = Cursors.Arrow;
                }
            }
        }

        #endregion // Excel export

        private async void btnBioEvalReport_Click(object sender, RoutedEventArgs e)
        {
            var reportDialog = new BioEvalReportDialog
            {
                PhysicianRequest = ViewModel.PhysicianRequest,
                PhysicsReportNotes = ViewModel.PhysicsReportNotes,
                PhysicianFinalAcknowledgment = ViewModel.PhysicianFinalAcknowledgment,
                ShowToDocumentsButton = true
            };

            if (reportDialog.ShowDialog() == true)
            {
                ViewModel.PhysicianRequest = reportDialog.PhysicianRequest;
                ViewModel.PhysicsReportNotes = reportDialog.PhysicsReportNotes;
                ViewModel.PhysicianFinalAcknowledgment = reportDialog.PhysicianFinalAcknowledgment;

                if (reportDialog.Action == BioEvalReportDialogAction.Show)
                {
                    ViewModel.ShowBioEvalReport();
                }
                else if (reportDialog.Action == BioEvalReportDialogAction.SaveToDocuments)
                {
                    await ViewModel.UploadBioEvalReportAsync();
                }
            }
        }

        private void ShowCustomMetricDialog(object sender, RoutedEventArgs e)
        {
            new MetricEditor().ShowDialog();
        }

        private void OnRecalculateClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedMetricRow == null)
            {
                return;
            }

            // Causes the metric name to include "[Changed]"
            DVHMetricSetupViewModel dvhMetricSetup = ViewModel.SelectedMetricRow.SelectedDVHMetricSetupViewModel;
            dvhMetricSetup.PropertiesChanged = true;

            ViewModel.SelectedMetricRow.CalculateForAllPlans();
        }

        private void OnCalculationNotification(object sender, CalcNotificationEventArgs e)
        {
            MessageBoxImage icon = MessageBoxImage.None;

            if (e.Type == CalcNotificationType.Error)
            {
                icon = MessageBoxImage.Error;
            }
            else if (e.Type == CalcNotificationType.Warning)
            {
                icon = MessageBoxImage.Warning;
            }

            MessageBox.Show(e.Message, "Calculation Notification",
                MessageBoxButton.OK, icon);
        }

        private void OnIsBusyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
            {
                if (ViewModel.IsBusy)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteMetricRow();
        }

        private void OnMetricRowSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MetricEditorViewModel metricEditorViewModel =
                MetricEditor.DataContext as MetricEditorViewModel;

            if (ViewModel.SelectedMetricRow != null)
            {
                metricEditorViewModel.SelectedDVHMetricSetup =
                    ViewModel.SelectedMetricRow.SelectedDVHMetricSetupViewModel;
            }
            else
            {
                metricEditorViewModel.SelectedDVHMetricSetup = null;
            }
        }

        private void ApplySelectedTemplate(object sender, RoutedEventArgs e)
        {
            TemplateViewModel templateViewModel =
                TemplatesComboBox.SelectedItem as TemplateViewModel;

            if (templateViewModel != null)
            {
                TemplateMatchingDialog templateMatchingDialog = new TemplateMatchingDialog
                {
                    Template = templateViewModel.Template,
                    PatientStructureIds = ViewModel.StructureViewModels.Select(s => s.Id).ToArray()
                };

                if (templateMatchingDialog.ShowDialog() == true)
                {
                    var structureMatches = templateMatchingDialog.StructureMatches;

                    // Temporarily stop handling notifications, so that no message box
                    // appears while the calc window handles the notifications
                    ViewModel.CalculationNotification -= OnCalculationNotification;
                    ViewModel.ApplyTemplate(templateViewModel, structureMatches);
                    ViewModel.CalculationNotification += OnCalculationNotification;
                }
            }
        }

        private void CreateTemplate(object sender, RoutedEventArgs e)
        {
            TemplateDialog templateDialog = new TemplateDialog(ViewModel.User.Id);

            TemplateViewModel templateViewModel =
                templateDialog.ViewModel.CreateTemplate(ViewModel);
            templateDialog.ViewModel.AddTemplate(templateViewModel);
            templateDialog.TemplateListBox.SelectedItem = templateViewModel;

            if (templateDialog.ShowDialog() == true)
            {
                ViewModel.TemplatesViewModel = templateDialog.ViewModel;
            }
        }

        private void ShowTemplatesDialog(object sender, RoutedEventArgs e)
        {
            TemplateDialog templateDialog = new TemplateDialog(ViewModel.User.Id);

            if (templateDialog.ShowDialog() == true)
            {
                ViewModel.TemplatesViewModel = templateDialog.ViewModel;
            }
        }

        // Warn users that LQL is experimental when they close the
        // drop-down box because doing it at the DVHModel level causes
        // too many warnings (since the model may be changed in other ways)
        private void OnDVHModelTypeComboBoxDropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox != null)
            {
                if (comboBox.SelectedItem is DVHModelTypes)
                {
                    DVHModelTypes dvhModelType = (DVHModelTypes)comboBox.SelectedItem;

                    if (dvhModelType == DVHModelTypes.LQLBioDose)
                    {
                        ViewModel.NotifyUserMessaged("Warning",
                            "The LQ-L bio-correction model is experimental " +
                            "and should not be used without clinical oversight.",
                            UserMessageType.Warning);
                    }
                }
            }
        }

        
    }
}
