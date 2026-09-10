using MRoar_Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UMRO.DvhAnalysis.AriaDb;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using UMRO.DvhAnalysis.Script.Presentation.Converters;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for SaveToDBWindow.xaml
    /// </summary>
    public partial class SaveToDBWindow : Window
    {
        public SaveToDBWindow(ViewModel vm, DataGrid mainWindowDataGridMetrics)
        {
            InitializeComponent();

            DataContext = vm;

            ViewModel = vm;

            this.main_wind_dg = mainWindowDataGridMetrics;
        }

        private static readonly ILogger _logger = AppLog.GetLogger(nameof(SaveToDBWindow));


        private ViewModel ViewModel;

        private readonly DataGrid main_wind_dg;


        private void SaveToDBWindowLoaded(object sender, RoutedEventArgs e)
        {
            //InitializeMetricDataGrid();

            InitializeMetricDataGrid_2();

        }


        private void InitializeMetricDataGrid_2()
        {
            var mConverter = new MetricResultConverter();

            //ViewModel.MetricRows.First().MetricResults.First()

            //var template = ViewModel.TemplatesViewModel; -- this is not the selected Template. It contains all the 1000+ templates, 

            List<string> visible_plan_names = ViewModel.PlanViewModels.Where(t => t.IsSelected).Select(t => t.Name).ToList();



            ObservableCollection<metric_row> DGsource = new ObservableCollection<metric_row>();

            var ordered_selected_metric_columns = main_wind_dg.Columns.
                Where(t =>
                    !t.IsFrozen &&
                    visible_plan_names.Contains(t.Header.ToString()))
                .OrderBy(t => t.DisplayIndex).ToList();


            //foreach (var Mrow in ViewModel.MetricRows)
            foreach (var _Mrow in main_wind_dg.Items)
            {
                var Mrow = _Mrow as MetricRowViewModel;

                if (Mrow.SelectedDVHMetricSetupViewModel == null) continue;

                var mr = new metric_row(Mrow.SelectedDVHMetricSetupViewModel.Name, Mrow.StructureViewModel.Id);

                mr.Metrics = new List<string>();


                foreach (var col1 in ordered_selected_metric_columns)
                {
                    // Add something that checks the plansetup with the same names. n> 1, n<1
                    // Actually dealed with this in the very start of the script. Directly complain and quit.

                    var metricRes1 = Mrow.MetricResults.Single(t => t.EclipseData.Plan.Id == col1.Header.ToString());

                    mr.Metrics.Add((string)mConverter.Convert_noUnit(metricRes1, null, null, null));
                }

                //for (int i = 0; i < plans.Count; i++)
                //{
                //    mr.Metrics.Add((string)mConverter.Convert_noUnit(Mrow.MetricResults[i], null, null, null));
                //}

                DGsource.Add(mr);
            }

            dtaMetrics.ItemsSource = DGsource;
            //dtaMetrics.ItemsSource = main_wind_dg.Items;

            dtaMetrics.Columns.Add(
               new DataGridTextColumn
               {
                   Header = CS.Structure,
                   Binding = new Binding(CS.Structure),
                   IsReadOnly = true
               });

            dtaMetrics.Columns.Add(
                new DataGridTextColumn
                {
                    Header = CS.MetricName,
                    Binding = new Binding(CS.MetricName),
                    IsReadOnly = true
                });


            //for (int i = 1; i < main_wind_dg.Columns.Count; i++)
            //{
            //    var col1 = main_wind_dg.Columns[i];

            //    int disp_i = col1.DisplayIndex;

            //    DataGridTextColumn metricCol_2 = new DataGridTextColumn
            //    {
            //        Header = col1.Header,
            //        Binding = new Binding("MetricResults[" + i + "]")
            //        {
            //            Converter = new MetricResultConverter()
            //        }
            //    };

            //    dtaMetrics.Columns.Add(metricCol_2);
            //}

            for (int i = 0; i < ordered_selected_metric_columns.Count; i++)
            {
                DataGridTextColumn metricCol = new DataGridTextColumn
                {
                    Header = ordered_selected_metric_columns[i].Header,
                    Binding = new Binding("Metrics[" + i + "]"),
                    IsReadOnly = true
                };

                dtaMetrics.Columns.Add(metricCol);
            }


            var comment_col = new DataGridTemplateColumn
            {
                Header = CS.Comment,
                //Binding = new Binding(CS.Comment),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star), // Takes available space "*"
                MaxWidth = 250,
                MinWidth = 80,
                IsReadOnly = false
            };


            // Create the CellTemplate (for displaying the text)
            var cellTemplate = new DataTemplate();
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Comment"));
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            cellTemplate.VisualTree = textBlockFactory;
            comment_col.CellTemplate = cellTemplate;

            // Create the CellEditingTemplate (for editing the text)
            var cellEditingTemplate = new DataTemplate();
            var textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
            textBoxFactory.SetBinding(TextBox.TextProperty, new Binding("Comment"));
            textBoxFactory.SetValue(TextBox.TextWrappingProperty, TextWrapping.Wrap);
            textBoxFactory.SetValue(TextBox.AcceptsReturnProperty, true);  // Allows multiline editing
            cellEditingTemplate.VisualTree = textBoxFactory;
            comment_col.CellEditingTemplate = cellEditingTemplate;


            //comment_col.CellStyle = (Style)FindResource("WrappedTextCellStyle");

            dtaMetrics.Columns.Add(comment_col);

        }


        public class metric_row
        {
            public metric_row(string MetricName, string structureName)
            {
                this.MetricName = MetricName;
                Structure = structureName;
            }

            public string MetricName { get; set; }
            public string Structure { get; set; }
            public string Comment { get; set; } = ""; // interestingly, you do not need raise PropertyChanged to have the UI bind back.

            public List<string> Metrics { get; set; }
        }



        //private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        //{
        //    Button srcButton = e.Source as Button;

        //    var row = Helper.FindParent<DataGridRow>(srcButton);

        //    ViewModel.MetricRows.Remove(row.DataContext as MetricRowViewModel);

        //    ViewModel.SelectedMetricRow = null;

        //    ViewModel.UpdateVolumes();
        //}


        private void Button_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            dtaMetrics.UnselectAllCells();

            int n_col = dtaMetrics.Columns.Count;

            int n_row = dtaMetrics.Items.Count;

            foreach (var row in dtaMetrics.Items)
            {
                for (int j = 2; j < n_col; j++)
                {
                    var xx = new DataGridCellInfo(row, dtaMetrics.Columns[j]);

                    dtaMetrics.SelectedCells.Add(xx);
                }
            }

        }

        private List<DataGridCellInfo> _SelectAllMetricCells()
        {
            List<DataGridCellInfo> all_cells = new List<DataGridCellInfo>();

            int n_col = dtaMetrics.Columns.Count;

            foreach (var row in dtaMetrics.Items)
            {
                for (int j = 2; j < n_col; j++)
                {
                    if (dtaMetrics.Columns[j].Header.ToString() == CS.Comment) continue;

                    var xx = new DataGridCellInfo(row, dtaMetrics.Columns[j]);

                    all_cells.Add(xx);
                }
            }

            return all_cells;
        }

        private void Button_UnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            dtaMetrics.UnselectAllCells();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            IList<DataGridCellInfo> selected_cells = dtaMetrics.SelectedCells;

            var all_MetricValue_cells = _SelectAllMetricCells();

            var cell1 = (all_MetricValue_cells.First().Item as metric_row);



            if (selected_cells.Count > 0)
            {
                string str_grammer = selected_cells.Count == 1 ? "metric is" : "metrics are";

                var res = MessageBox.Show($"{selected_cells.Count} {str_grammer} selected. And they will be marked as metrics being used for clinical decision.\n\nDo you want to proceed?", "DVH Analysis", MessageBoxButton.OKCancel);

                if (res == MessageBoxResult.Cancel) return;
            }

            var now = DateTime.Now;

            string userid = ViewModel.User.Id;

            var planVMs = ViewModel.PlanViewModels.Where(t => t.IsSelected).ToList();

            _logger.Info($"Save to ROAR started: MRN {ViewModel.Patient.Id}, " +
                         $"{planVMs.Count} plan(s) selected, {selected_cells.Count} clinical-decision cell(s)");

            //string msg = planVMs.check_if_all_compoments_loaded();

            //if (!string.IsNullOrEmpty(msg))
            //{
            //    var res = MessageBox.Show(msg +
            //        "\n\nUn-loaded or hidden metrics won't be saved into database." +
            //        "\n\nDo you still want to continue?",
            //        "Save to Database - DVH Analysis", MessageBoxButton.OKCancel);

            //    if (res == MessageBoxResult.Cancel) return;
            //}



            Patient pt = ViewModel.Patient;

            var cs = ViewModel.Course;

            // Use the ROAR connection string from the plugin config if provided (single place to
            // set the server at deploy time); otherwise cannot save Metrics to DB

            var roarConn = AssemblySettings.RoarConnectionString;

            if (string.IsNullOrWhiteSpace(roarConn))
            {
                MessageBox.Show("Connection string [RoarConnString] to a database is not set in config. \n\nCannot save metrics to database.", "DVH Analysis");

                return;
            }

            var dc = new MRoar_Database.ROARFormEntryDataContext(roarConn);

            AboutPatient aboutPat = ViewModel.InfoQueryRepository.LoadPatientInfoById(pt.Id);

            if (aboutPat == null)
            {
                throw new Exception($"Cannot load Patient info for MRN {pt.Id}");
            }


            var planVMs_psum = planVMs.Where(t => t.IsPlanSum).ToList();

            var planVMs_psetup = planVMs.Where(t => !t.IsPlanSum).ToList();

            var instance = new DVHAnalysis_DataEnterInstance()
            {
                MRN = pt.Id,
                db_entry_time = now,
                db_entry_userid = userid,
                IsReIrradiationEvaluation = CBox_isReIrradiation.IsChecked
            };

            dc.DVHAnalysis_DataEnterInstances.InsertOnSubmit(instance);

            dc.SubmitChanges();


            foreach (var pl in planVMs_psum)
            {
                var PSum = pl.Plan as PlanSum;

                AboutCourse aboutCous = ViewModel.InfoQueryRepository.LoadCourseInfo(aboutPat.PatientSer, pl.Plan.GetCourse().Id);

                if (aboutCous == null)
                {
                    throw new Exception($"Cannot load Course info for {pl.Plan.Id}");
                }

                AboutPlanSum aboutPSum = ViewModel.InfoQueryRepository.LoadPlanSumInfo(aboutCous.CourseSer, PSum.Id);

                if (aboutPSum == null)
                {
                    throw new Exception($"Cannot load PlanSum info for {PSum.Id}");
                }

                List<string> compoment_IDs = PSum.PlanSumComponents.OrderBy(t => t.PlanSetupId).Select(t => t.PlanSetupId).ToList();

                List<string> compoment_IDs_n_Weights = PSum.PlanSumComponents.OrderBy(t => t.PlanSetupId).Select(t => $"{t.PlanSetupId} [{t.PlanWeight}]").ToList();


                var Eval_plan = new DVHAnalysis_PlanSum()
                {
                    Instance_ID = instance.Instance_ID,
                    PlanSumID = PSum.Id,
                    CompomentPlanSetupIDs = string.Join("; ", compoment_IDs_n_Weights),

                    ARIA_PlanSumSer = aboutPSum.PlanSumSer,
                    MRN = pt.Id,
                    ARIA_PatientSer = aboutPat.PatientSer,
                    ARIA_CourseSer = aboutCous.CourseSer,
                    CourseID = aboutCous.CourseID,

                    CreationDateTime = PSum.CreationDateTime,

                    db_entry_time = now,
                    db_entry_userid = userid
                };

                dc.DVHAnalysis_PlanSums.InsertOnSubmit(Eval_plan);

                dc.SubmitChanges();


                // Save cells in this column
                var col_1 = all_MetricValue_cells.Where(t => t.Column.Header.ToString() == PSum.Id).ToList();

                foreach (var cell in col_1)
                {
                    metric_row row = cell.Item as metric_row;

                    var xxx = row.Metrics[cell.Column.DisplayIndex - n_col_offset];

                    if (string.IsNullOrEmpty(xxx) || xxx == "NaN") continue;

                    DVHAnalysis_Metric metric_1 = create_metric_entry(
                        Eval_plan.PlanSum_ID,
                        true, row, cell, PSum.StructureSet.Id,
                        instance.Instance_ID);

                    if (selected_cells.Contains(cell)) metric_1.IsReIrradiationEvaluation = true;

                    metric_1.db_entry_time = now;
                    metric_1.db_entry_userid = userid;

                    dc.DVHAnalysis_Metrics.InsertOnSubmit(metric_1);
                }

                dc.SubmitChanges();


                if (false) // The following will cause duplication in PlanSetup table. If needed, this should be saved into a seperate table called PlanSum_Compoments.
                {
                    foreach (string com_id in compoment_IDs)
                    {
                        PlanViewModel planVM_setup_1 = planVMs_psetup.SingleOrDefault(t => t.Plan.Id == com_id && t.Plan.GetCourse().Id == aboutCous.CourseID && t.HasBeenSavedToDB == false);

                        if (planVM_setup_1 == null) continue;

                        var PSetup = planVM_setup_1.Plan as PlanSetup;

                        var comp_in_psum = PSum.PlanSumComponents.Single(t => t.PlanSetupId == com_id);

                        AboutPlanSetup aboutPSetup = ViewModel.InfoQueryRepository.LoadPlanSetupInfo(aboutCous.CourseSer, PSetup.Id);

                        if (aboutPSetup == null)
                        {
                            throw new Exception($"Cannot load PlanSetup info for {PSetup.Id}");
                        }

                        var comp_plan = new DVHAnalysis_PlanSetup()
                        {
                            Instance_ID = instance.Instance_ID,

                            PlanSetupID = PSetup.Id,

                            PlanSumID = PSum.Id,
                            PlanSum_ID = Eval_plan.PlanSum_ID,

                            MRN = pt.Id,
                            ARIA_PatientSer = aboutPat.PatientSer,
                            ARIA_CourseSer = aboutCous.CourseSer,
                            CourseID = aboutCous.CourseID,

                            ARIA_PlanSetupSer = aboutPSetup.PlanSetupSer,

                            Component_Weight = comp_in_psum.PlanWeight,
                            Component_NFractionsPlanned = PSetup.NumberOfFractions,

                            IsVolumetricDoseComponent = true,
                            IsPaperChartBasedComponent = false,


                            CreationDateTime = PSetup.CreationDateTime,

                            db_entry_time = now,
                            db_entry_userid = userid
                        };

                        dc.DVHAnalysis_PlanSetups.InsertOnSubmit(comp_plan);

                        dc.SubmitChanges();

                        planVM_setup_1.HasBeenSavedToDB = true;


                        // Save cells in this column
                        var col_11 = all_MetricValue_cells.Where(t => t.Column.Header.ToString() == PSetup.Id).ToList();

                        foreach (var cell in col_11)
                        {
                            metric_row row = cell.Item as metric_row;

                            var xxx = row.Metrics[cell.Column.DisplayIndex - n_col_offset];

                            if (string.IsNullOrEmpty(xxx) || xxx == "NaN") continue;

                            DVHAnalysis_Metric metric_1 = create_metric_entry(
                                comp_plan.PlanSetup_ID,
                                false, row, cell, PSetup.StructureSet.Id,
                                instance.Instance_ID);

                            if (selected_cells.Contains(cell)) metric_1.IsReIrradiationEvaluation = true;

                            metric_1.db_entry_time = now;
                            metric_1.db_entry_userid = userid;

                            dc.DVHAnalysis_Metrics.InsertOnSubmit(metric_1);
                        }

                        dc.SubmitChanges();

                    }
                }
            }


            foreach (var pl in planVMs_psetup.Where(t => t.HasBeenSavedToDB == false).ToList())
            {
                var PSetup = pl.Plan as PlanSetup;

                AboutCourse aboutCous = ViewModel.InfoQueryRepository.LoadCourseInfo(aboutPat.PatientSer, pl.Plan.GetCourse().Id);

                if (aboutCous == null)
                {
                    throw new Exception($"Cannot load Course info for {pl.Plan.Id}");
                }

                AboutPlanSetup aboutPSetup = ViewModel.InfoQueryRepository.LoadPlanSetupInfo(aboutCous.CourseSer, PSetup.Id);

                if (aboutPSetup == null)
                {
                    throw new Exception($"Cannot load PlanSetup info for {PSetup.Id}");
                }

                var single_planSetup = new DVHAnalysis_PlanSetup()
                {
                    Instance_ID = instance.Instance_ID,

                    PlanSetupID = PSetup.Id,

                    ARIA_PlanSetupSer = aboutPSetup.PlanSetupSer,

                    MRN = pt.Id,
                    ARIA_PatientSer = aboutPat.PatientSer,
                    ARIA_CourseSer = aboutCous.CourseSer,
                    CourseID = aboutCous.CourseID,

                    CreationDateTime = PSetup.CreationDateTime,

                    db_entry_time = now,
                    db_entry_userid = userid,


                    Component_NFractionsPlanned = PSetup.NumberOfFractions,

                    IsVolumetricDoseComponent = true,
                    IsPaperChartBasedComponent = false

                };

                dc.DVHAnalysis_PlanSetups.InsertOnSubmit(single_planSetup);

                dc.SubmitChanges();



                var col_1 = all_MetricValue_cells.Where(t => t.Column.Header.ToString() == PSetup.Id).ToList();

                foreach (var cell in col_1)
                {
                    metric_row row = cell.Item as metric_row;

                    var xxx = row.Metrics[cell.Column.DisplayIndex - n_col_offset];

                    if (string.IsNullOrEmpty(xxx) || xxx == "NaN") continue;

                    // you cannot easily grab the value of a cell in WPF.
                    // You need to use the data source to get the value.
                    DVHAnalysis_Metric metric_1 = create_metric_entry(
                        single_planSetup.PlanSetup_ID,
                        false, row, cell, PSetup.StructureSet.Id,
                        instance.Instance_ID);

                    if (selected_cells.Contains(cell)) metric_1.IsReIrradiationEvaluation = true;

                    metric_1.db_entry_time = now;
                    metric_1.db_entry_userid = userid;

                    dc.DVHAnalysis_Metrics.InsertOnSubmit(metric_1);
                }

                dc.SubmitChanges();
            }

            _logger.Info($"Save to ROAR completed: MRN {pt.Id}, instance {instance.Instance_ID}");

            MessageBox.Show("Selected Metrics Saved.", "DVH Analysis");

            this.Close();

            //foreach (DataGridCellInfo cell in cells)
            //{
            //    string header = cell.Column.Header as string;
            //    metric_row row = cell.Item as metric_row;
        }


        private const int n_col_offset = 2;

        private DVHAnalysis_Metric create_metric_entry(int PlanOrPlanSum_ID, bool IsPlanSum, metric_row row, DataGridCellInfo cell, string StructureSetID, int instance_id)
        {
            var metric_1 = new MRoar_Database.DVHAnalysis_Metric()
            {
                Instance_ID = instance_id,
                StructureID = row.Structure,
                DVHMetricName = row.MetricName,

                // you cannot easily grab the value of a cell in WPF.
                // You need to use the data source to get the value.
                DVHMetricValue = double.Parse(row.Metrics[cell.Column.DisplayIndex - n_col_offset]),

                StructureSetID = StructureSetID, // how to get this from plansum?

                IsReIrradiationEvaluation = false,
                IsVolumetricDVHDoseEstimate = true,
                IsPointDoseEstimate = false,
                
                Comment = row.Comment,

                db_entry_source = $"{ViewModel.ProductName}_v{ViewModel.ProductVersion}"
            };

            if (IsPlanSum)
            {
                metric_1.PlanSum_ID = PlanOrPlanSum_ID;
            }
            else
            {
                metric_1.PlanSetup_ID = PlanOrPlanSum_ID;
            }

            return metric_1;
        }


        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_View_PDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;

                Patient pt = ViewModel.Patient;

                var pdf_exporter = new PDFExport_SaveToDBWindow($"{pt.FirstName} {pt.LastName}", pt.Id, pt.DateOfBirth, pt.Sex, ViewModel.User.Id);

                var return_path = pdf_exporter.ExportDataGridToPdf(dtaMetrics);

                Process.Start(return_path);

                Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate metrics PDF", ex);
                MessageBox.Show(
                    $"There was a problem generating the document. Error message: {ex.Message}",
                    "DVH Analysis - Error Export PDF");
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }


        private async void Button_Upload_PDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;

                Patient pt = ViewModel.Patient;

                var pdf_exporter = new PDFExport_SaveToDBWindow($"{pt.FirstName} {pt.LastName}", pt.Id, pt.DateOfBirth, pt.Sex, ViewModel.User.Id);

                var reportPath = pdf_exporter.ExportDataGridToPdf(dtaMetrics);

                var ariaDocClient = new AriaDocumentClient(ViewModel.User.Id, pt.Id);

                await ariaDocClient.InsertDocumentAsync(reportPath, "PDF", "Special Medical Physics Consultation");

                Cursor = Cursors.Arrow;

                ViewModel.NotifyUserMessaged("Success uploading document",
                    $"The document was successfully uploaded.", UserMessageType.Information);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate or upload metrics PDF to ARIA", ex);
                MessageBox.Show(
                    $"There was a problem generating or uploading the document. Error message: {ex.Message}",
                    "DVH Analysis - Error Upload PDF");
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }


    }



    public static class CS
    {
        public static string Structure = "Structure";
        public static string MetricName = "MetricName";
        public static string Comment = "Comment";
    }
}
