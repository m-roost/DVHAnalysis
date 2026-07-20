using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DVHAnalysis;
using Microsoft.Office.Interop.Excel;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using UMRO.DvhAnalysis.Script.Presentation.Converters;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Metrics;
using UMRO.DvhAnalysis.Script.QcAnalysis;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script
{
    public class ExcelDocumentCreator
    {
        private static readonly ILogger _log = AppLog.GetLogger(nameof(ExcelDocumentCreator));

        public ViewModel ViewModel { get; set; }

        public bool ExportDVH { get; set; }
        public bool ExportDVHTypeCumulative { get; set; }
        public bool ExportDVHDoseAbsolute { get; set; }
        public bool ExportDVHVolumeAbsolute { get; set; }
        public bool ExportQCAnalysis { get; set; }

        // Row numbers for DVH data in worksheet
        private const int TitleRowNumber = 1;
        private const int TitleColumnNumber = 1;
        private const int StructureNameRowNumber = 3;
        private const int DataTitleRowNumber = 4;
        private const int DataRowNumber = 5;
        private const int DataColumnSkip = 1;    // Number of columns to skip between data sets

        public void CreateDocument(List<string> colHeaders, List<int> planFractions, List<StructureViewModel> structures, List<PlanningItem> pitems)
        {
            _log.Info($"Excel export started: {structures.Count} structure(s), {colHeaders.Count} plan column(s), " +
                      $"DVH={ExportDVH}, QC={ExportQCAnalysis}");

            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
                throw new ApplicationException("Could not create Excel application");

            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet comparisonWs;

            // If doing QC analysis, put the main plan first,
            // then the QC plans sorted by ending two-digit number
            if (ExportQCAnalysis)
            {
                colHeaders = SortPlanIds(colHeaders);
            }

            WriteMetrics(wb, out comparisonWs, colHeaders, structures);

            if (ExportQCAnalysis)
            {
                WriteAnalysis(wb);
            }

            WriteVolumes(wb, colHeaders, structures);
            if (ExportDVH)
            {
                WriteDVHs(wb, colHeaders, structures, pitems);
            }

            wb.Worksheets[1].Select();

            xlApp.Visible = true;

            _log.Info("Excel export completed");
        }

        // Note: Some of this code is duplicated in the QcAnalyzer class (needs refactoring)
        private List<string> SortPlanIds(List<string> planIds)
        {
            var sortedIds = new List<string>();

            var mainPlanId = planIds.SingleOrDefault(id => !id.StartsWith("QC"));

            if (mainPlanId == null)
            {
                throw new InvalidOperationException(
                    "A single main plan for QC analysis was not found.");
            }

            sortedIds.Add(mainPlanId);

            var qcPlanIds = planIds.Where(id => id.StartsWith("QC")).OrderBy(GetQcNumber);

            sortedIds.AddRange(qcPlanIds);

            return sortedIds;
        }

        private int GetQcNumber(string id)
        {
            int idLength = id.Length;

            try
            {
                // QC plan IDs always end with a two-digit number
                return Convert.ToInt32($"{id[idLength - 2]}{id[idLength - 1]}");
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Unable to obtain the two-digit number from the plan {id}.");
            }
        }

        private void WriteMetrics(Workbook wb, out Worksheet ws, List<string> colHeaders, List<StructureViewModel> structures)
        {
            ws = (Worksheet)wb.Worksheets[1];
            if (ws == null)
                throw new ApplicationException("Could not create Excel worksheet");
            ws.Name = "Comparison";

            //the title
            Style titleStyle = wb.Styles.Add("TitleStyle");
            titleStyle.Font.Size = 14;
            titleStyle.Font.Bold = true;
            Range titleRange = (Range)ws.Cells[1, 5];
            titleRange.Style = titleStyle;
            titleRange.Value = string.Format("DVH Metric Comparison for Patient {0} ({1}, {2})",
                ViewModel.Patient.Id, ViewModel.Patient.LastName, ViewModel.Patient.FirstName);

            //The column headers
            Style headerStyle = wb.Styles.Add("HeaderStyle");
            headerStyle.Font.Bold = true;
            headerStyle.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
            Range headerRange = ws.Range[(Range)ws.Cells[3, 1], (Range)ws.Cells[3, colHeaders.Count + 2]];
            headerRange.Style = headerStyle;
            var headers = new string[colHeaders.Count + 2];
            headers[0] = "ROI";
            headers[1] = "Metric";
            for (int i = 0; i < colHeaders.Count; i++)
                headers[i + 2] = colHeaders[i];
            headerRange.Value2 = headers;

            //add data rows
            int iRow = 4;

            // Converter to convert metric result into a pretty string
            MetricResultExcelConverter metricConverter = new MetricResultExcelConverter();

            var metricRows = ViewModel.MetricRows.OrderBy(r => r.StructureViewModel.Id);
            foreach (MetricRowViewModel metricRow in metricRows)
            {
                DVHMetricSetupViewModel dvhMetricSetupViewModel =
                    metricRow.SelectedDVHMetricSetupViewModel;

                if (dvhMetricSetupViewModel == null)
                {
                    continue;
                }

                bool firstMetric = true;
                Range rowRange = ws.Range[(Range)ws.Cells[iRow, 1], (Range)ws.Cells[iRow, colHeaders.Count + 2]];
                var row = new object[colHeaders.Count + 2];
                if (firstMetric)
                    row[0] = metricRow.StructureViewModel.Id;
                else
                    row[0] = "";
                row[1] = dvhMetricSetupViewModel.Name;
                for (int i = 0; i < colHeaders.Count; i++)
                {
                    string field = colHeaders[i];
                    int j = (from plan in ViewModel.Plans
                             select plan.Id).ToList().IndexOf(field);

                    if (metricRow.MetricResults.Count > 0)
                    {
                        row[i + 2] = metricConverter.Convert(metricRow.MetricResults[j],
                            typeof(string), null, null);
                    }
                }
                rowRange.Value = row;

                string description = string.Empty;

                // Add parameters for NTCP and gEUD metrics
                if (dvhMetricSetupViewModel.MetricViewModel is NTCPMetricViewModel)
                {
                    NTCPMetricViewModel metric =
                        dvhMetricSetupViewModel.MetricViewModel as NTCPMetricViewModel;
                    description += string.Format("n = {0}, m = {1}, TD50 = {2}",
                        metric.LKBn, metric.LKBm, metric.LKBD50);
                }
                else if (dvhMetricSetupViewModel.MetricViewModel is EUDMetricViewModel)
                {
                    EUDMetricViewModel metric =
                        dvhMetricSetupViewModel.MetricViewModel as EUDMetricViewModel;
                    description += string.Format("a = {0}", metric.a);
                }

                // Add biodose parameters
                if (dvhMetricSetupViewModel.DVHModelViewModel is LQBioDoseDVHModelViewModel)
                {
                    LQBioDoseDVHModelViewModel dvhModel =
                        dvhMetricSetupViewModel.DVHModelViewModel as LQBioDoseDVHModelViewModel;

                    if (description != string.Empty)
                    {
                        description += ", ";
                    }

                    description += string.Format("LQ: \u03B1/\u03B2 = {0}", dvhModel.AlphaBeta);
                }
                else if (dvhMetricSetupViewModel.DVHModelViewModel is LQLBioDoseDVHModelViewModel)
                {
                    LQLBioDoseDVHModelViewModel dvhModel =
                        dvhMetricSetupViewModel.DVHModelViewModel as LQLBioDoseDVHModelViewModel;

                    if (description != string.Empty)
                    {
                        description += ", ";
                    }

                    description += string.Format("LQL: \u03B1/\u03B2 = {0}, DT = {1}",
                        dvhModel.AlphaBeta, dvhModel.DT);
                }

                ws.Cells[iRow, colHeaders.Count + 4] = description;

                firstMetric = false;
                iRow++;
            }


            //autofit table columns
            Range allRange = ws.Range[(Range)ws.Cells[3, 1], (Range)ws.Cells[999, 999]];
            allRange.Columns.AutoFit();

            //format all numeric cells
            Range numbersRange = ws.Range[(Range)ws.Cells[3, 3], (Range)ws.Cells[999, 999]];
            numbersRange.NumberFormat = "0.00";
        }

        private void WriteAnalysis(Workbook wb)
        {
            // QC Analysis goes into its own sheet
            Worksheet ws = (Worksheet)wb.Worksheets.Add();
            ws.Name = "QC Analysis";

            // This appears to move the sheet to the end
            ws.Move(Missing.Value, wb.Worksheets[wb.Worksheets.Count]);

            // Make all the columns the same width (looks good)
            ws.Columns.ColumnWidth = 18;

            // Starting row on worksheet
            int row = 1;

            row = WriteTitle(ws, row);

            // Perform the QC analysis
            var qcAnalyzer = new QcAnalyzer(ViewModel);
            var qcResult = qcAnalyzer.Analyze();

            // Note: Each write method is sent row + 1
            // so that there is a space in between sections

            row = WritePlanFractions(qcResult, ws, row + 1);
            row = WriteMetricValueTable(qcResult, ws, row + 1);
            row = WriteDailyFractionTable(qcResult, ws, row + 1);
            row = WritePercentDailyDeviationTable(qcResult, ws, row + 1);
            row = WriteAbsoluteDoseDeviationTable(qcResult, ws, row + 1);
            row = WriteSummaryTable(qcResult, ws, row + 1);
        }

        private int WriteTitle(Worksheet ws, int row)
        {
            ((Range)ws.Cells[row, 1]).Value = "DVH Metric QC Analysis";
            ((Range)ws.Cells[row, 1]).Font.Size = 14;
            ((Range)ws.Cells[row, 1]).Font.Bold = true;
            return row + 1;
        }

        private int WritePlanFractions(QcResult qcResult, Worksheet ws, int row)
        {
            ((Range)ws.Cells[row, 1]).Value = "Plan ID";
            ((Range)ws.Cells[row, 1]).Font.Bold = true;
            ((Range)ws.Cells[row, 2]).Value = qcResult.MainPlan.Id;
            row++;

            ((Range)ws.Cells[row, 1]).Value = "# of Fx";
            ((Range)ws.Cells[row, 1]).Font.Bold = true;
            ((Range)ws.Cells[row, 2]).Value = qcResult.MainPlanFractions;
            row++;

            return row;
        }

        private int WriteMetricValueTable(QcResult qcResult, Worksheet ws, int row)
        {
            return WriteQcTable(qcResult.MetricValueTable, "Complete Tx", ws, row);
        }

        private int WriteDailyFractionTable(QcResult qcResult, Worksheet ws, int row)
        {
            return WriteQcTable(qcResult.DailyFractionTable, "Daily Fx", ws, row);
        }

        private int WritePercentDailyDeviationTable(QcResult qcResult, Worksheet ws, int row)
        {
            return WriteQcTable(qcResult.PercentDailyDeviationTable,
                "% Deviation Daily", ws, row, asPercent: true);
        }

        private int WriteAbsoluteDoseDeviationTable(QcResult qcResult, Worksheet ws, int row)
        {
            return WriteQcTable(qcResult.AbsoluteDoseDeviationTable,
                "Absolute Dose Deviation of Plan", ws, row, includeMax: true);
        }

        private int WriteQcTable(QcTable table, string title, Worksheet ws, int row, bool asPercent = false, bool includeMax = false)
        {
            Range tableTitleRange = (Range)ws.Cells[row++, 1];
            tableTitleRange.Value = title;
            tableTitleRange.Font.Bold = true;

            ((Range)ws.Cells[row, 1]).Value = "ROI";
            ((Range)ws.Cells[row, 2]).Value = "Metric";

            var planIds = table.Columns.Select(c => c.PlanId).ToArray();

            int col = 3;
            foreach (var planId in planIds)
            {
                ((Range)ws.Cells[row, col++]).Value = planId;
            }

            // Style the header row (not including AVG or MAX)
            var headerRange = ws.Range[ws.Cells[row, 1], ws.Cells[row, planIds.Length + 2]];
            ApplyHeaderStyle(headerRange);

            ((Range)ws.Cells[row, col]).Value = "AVG";
            ApplySummaryHeaderStyle((Range)ws.Cells[row, col]);
            col++;

            if (includeMax)
            {
                ((Range)ws.Cells[row, col]).Value = "MAX";
                ApplySummaryHeaderStyle((Range)ws.Cells[row, col]);
            }

            row++;

            foreach (var tableRow in table.Rows)
            {
                ((Range)ws.Cells[row, 1]).Value = tableRow.StructureId;
                ((Range)ws.Cells[row, 2]).Value = tableRow.MetricName;

                col = 3;
                foreach (var planId in planIds)
                {
                    ((Range)ws.Cells[row, col++]).Value =
                        tableRow[planId].ToString(asPercent ? "P2" : "F2");
                }

                // Write average
                ((Range)ws.Cells[row, col++]).Value =
                    GetQcAverage(tableRow).ToString(asPercent ? "P2" : "F2");

                if (includeMax)
                {
                    ((Range)ws.Cells[row, col]).Value =
                        GetQcMax(tableRow).ToString(asPercent ? "P2" : "F2");
                }

                row++;
            }

            return row;
        }

        private double GetQcAverage(QcRow tableRow)
        {
            // The first element is the main plan, so skip it
            var values = tableRow.Values.Skip(1).ToArray();
            return values.Any() ? values.Average() : 0;
        }

        private double GetQcMax(QcRow tableRow)
        {
            // The first element is the main plan, so skip it
            var values = tableRow.Values.Skip(1).ToArray();
            return values.Any() ? values.Max() : 0;
        }

        private double GetQcStdDev(QcRow tableRow)
        {
            // The first element is the main plan, so skip it
            var values = tableRow.Values.Skip(1).ToArray();
            return values.Any() ? CalculateStdDev(values) : 0;
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            var array = values as double[] ?? values.ToArray();

            var mean = array.Average();

            var sumOfSquares = array.Select(x => (x - mean) * (x - mean)).Sum();

            return Math.Sqrt(sumOfSquares / (array.Length - 1.0));
        }

        private int WriteSummaryTable(QcResult qcResult, Worksheet ws, int row)
        {
            var patientName = qcResult.MainPlan.GetCourse().Patient.LastName + ", " +
                              qcResult.MainPlan.GetCourse().Patient.FirstName;
            var patientId = qcResult.MainPlan.GetCourse().Patient.Id;

            Range tableTitleRange = (Range)ws.Cells[row++, 1];
            tableTitleRange.Value = "Summary (+value = increased delivered dose)";
            tableTitleRange.Font.Bold = true;

            row++;

            ((Range)ws.Cells[row++, 1]).Value = $"{patientName} (Id: {patientId})";

            // Headers
            ((Range)ws.Cells[row, 1]).Value = "ROI";
            ((Range)ws.Cells[row, 2]).Value = "Metric";
            ((Range)ws.Cells[row, 3]).Value = qcResult.MainPlan + " Planned Dose";
            ((Range)ws.Cells[row, 3]).WrapText = true;
            ((Range)ws.Cells[row, 4]).Value = "Average Dose Difference [%]";
            ((Range)ws.Cells[row, 4]).WrapText = true;
            ((Range)ws.Cells[row, 5]).Value = "Total Dose Deviation [Gy]";
            ((Range)ws.Cells[row, 5]).WrapText = true;
            ((Range)ws.Cells[row, 6]).Value = "Standard Deviation Dose Difference [%]";
            ((Range)ws.Cells[row, 6]).WrapText = true;
            ((Range)ws.Cells[row, 7]).Value = "Expected Deviation of Delivered Dose";
            ((Range)ws.Cells[row, 7]).WrapText = true;
            ((Range)ws.Cells[row, 8]).Value = "Max Deviation of Delivered Dose";
            ((Range)ws.Cells[row, 8]).WrapText = true;
            ((Range)ws.Cells[row, 9]).Value = "Final Expected Dose Delivered [Gy]";
            ((Range)ws.Cells[row, 9]).WrapText = true;

            ApplyHeaderStyle(ws.Range[ws.Cells[row, 1], ws.Cells[row, 9]]);

            // For some reason, Excel makes the height of this row too big,
            // so restrict the size to two text line heights
            ws.Range[ws.Cells[row, 1], ws.Cells[row, 9]].RowHeight = 30;

            row++;

            foreach (var tableRow in qcResult.MetricValueTable.Rows)
            {
                ((Range)ws.Cells[row, 1]).Value = tableRow.StructureId;
                ((Range)ws.Cells[row, 2]).Value = tableRow.MetricName;

                double plannedDose = tableRow[0];
                ((Range)ws.Cells[row, 3]).Value = tableRow[0].ToString("F2");

                QcRow rowInPercentDailyDeviationTable =
                    qcResult.PercentDailyDeviationTable.GetRow(tableRow.StructureId, tableRow.MetricName);
                double averageDeviation = GetQcAverage(rowInPercentDailyDeviationTable);
                ((Range)ws.Cells[row, 4]).Value = averageDeviation.ToString("P2");
                double doseDeviation = plannedDose * averageDeviation;
                ((Range)ws.Cells[row, 5]).Value = (averageDeviation * tableRow[0]).ToString("F2");
                ((Range)ws.Cells[row, 6]).Value = GetQcStdDev(rowInPercentDailyDeviationTable).ToString("P2");

                QcRow rowInAbsoluteDoseDeviationTable =
                    qcResult.AbsoluteDoseDeviationTable.GetRow(tableRow.StructureId, tableRow.MetricName);
                ((Range)ws.Cells[row, 7]).Value = GetQcAverage(rowInAbsoluteDoseDeviationTable).ToString("F2");
                ((Range)ws.Cells[row, 8]).Value = GetQcMax(rowInAbsoluteDoseDeviationTable).ToString("F2");

                ((Range)ws.Cells[row, 9]).Value = (plannedDose + doseDeviation).ToString("F2");

                row++;
            }

            row++;

            ((Range)ws.Cells[row, 1]).Value = "This is a confidential Quality Improvement and Assurance/peer review document of the University of Michigan Hospitals and Health Centers.";

            row++;

            return row;
        }

        private void ApplyHeaderStyle(Range range)
        {
            range.Font.Bold = true;
            range.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
        }

        // Used for AVG and MAX
        private void ApplySummaryHeaderStyle(Range range)
        {
            range.Font.Bold = true;
            range.Interior.Color = ColorTranslator.ToOle(Color.DarkGray);
        }

        private void WriteVolumes(Workbook wb, List<string> colHeaders, List<StructureViewModel> structures)
        {
            Worksheet ws = (Worksheet)wb.Worksheets.Add();
            if (ws == null)
                throw new ApplicationException("Could not create Excel worksheet");
            ws.Move(System.Reflection.Missing.Value, wb.Worksheets[wb.Worksheets.Count]);
            ws.Name = "Volumes";

            //the title
            Style titleStyle = wb.Styles.Item[1];
            titleStyle.Font.Size = 14;
            titleStyle.Font.Bold = true;
            Range titleRange = (Range)ws.Cells[1, 5];
            titleRange.Style = titleStyle;
            titleRange.Value = "Volume Comparison(cc)";

            //The column headers
            Style headerStyle = wb.Styles.Item[2];
            headerStyle.Font.Bold = true;
            headerStyle.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
            Range headerRange = ws.Range[(Range)ws.Cells[3, 1], (Range)ws.Cells[3, colHeaders.Count + 2]];
            headerRange.Style = headerStyle;
            var headers = new string[colHeaders.Count + 2];
            headers[0] = "ROI";
            for (int i = 0; i < colHeaders.Count; i++)
                headers[i + 1] = colHeaders[i];
            headerRange.Value2 = headers;

            //add data rows
            int iRow = 4;
            foreach (VolumeRowViewModel volumeRow in ViewModel.Volumes)
            {
                Range rowRange = ws.Range[(Range)ws.Cells[iRow, 1], (Range)ws.Cells[iRow, colHeaders.Count + 2]];
                var row = new object[colHeaders.Count + 2];
                row[0] = volumeRow.Structure.Id;
                for (int i = 0; i < colHeaders.Count; i++)
                {
                    string field = colHeaders[i];
                    int j = (from plan in ViewModel.Plans
                             select plan.Id).ToList().IndexOf(field);
                    row[i + 1] = volumeRow.Volumes[j];
                }
                rowRange.Value = row;
                iRow++;
            }


            //autofit table columns
            Range allRange = ws.Range[(Range)ws.Cells[3, 1], (Range)ws.Cells[999, 999]];
            allRange.Columns.AutoFit();

            //format all numeric cells
            Range numbersRange = ws.Range[(Range)ws.Cells[3, 3], (Range)ws.Cells[999, 999]];
            numbersRange.NumberFormat = "0.00";
        }

        #region Write DVHs

        // TODO: Refactor to use DisplayDvhs class
        private void WriteDVHs(Workbook wb, List<string> colHeaders, List<StructureViewModel> structures, List<PlanningItem> plans)
        {
            foreach (string header in colHeaders)
            {
                PlanningItem plan = plans.FirstOrDefault(s => s.Id == header);
                if (plan == null)
                {
                    continue;
                }

                Worksheet ws = wb.Worksheets.Add();
                if (ws == null)
                {
                    throw new ApplicationException("Could not create Excel worksheet.");
                }

                ws.Move(Missing.Value, wb.Worksheets[wb.Worksheets.Count]);
                ws.Name = CreateValidWorksheetName(header);

                WriteDVHs(wb, ws, plan);
            }
        }

        private void WriteDVHs(Workbook wb, Worksheet ws, PlanningItem plan)
        {
            WriteTitle(wb, ws, plan);

            Chart standardChart = null;
            Chart bioChart = null;

            int colStart = 1;

            IEnumerable<Structure> structures = GetSelectedStructures(plan);
            foreach (Structure structure in structures)
            {
                DVH standardDVH = GetStandardDVH(plan, structure);

                if (standardDVH == null)
                {
                    continue;
                }

                standardChart = GetOrCreateStandardChart(ws, standardChart, standardDVH.DoseUnit, standardDVH.VolumeUnit);
                WriteAndPlotDVH(wb, ws, colStart, standardChart, standardDVH, structure.Id + " (Standard DVH)");

                colStart += 2 + DataColumnSkip;

                if (StructureHasLQDose(structure))
                {
                    DVH lqDVH = GetLQDVH(plan, structure);

                    bioChart = GetOrCreateBioChart(ws, standardChart, bioChart, standardDVH, lqDVH);
                    WriteAndPlotDVH(wb, ws, colStart, bioChart, lqDVH, structure.Id + " (LQ DVH)");

                    colStart += 2 + DataColumnSkip;
                }

                if (StructureHasLQLDose(structure))
                {
                    DVH lqlDVH = GetLQLDVH(plan, structure);

                    bioChart = GetOrCreateBioChart(ws, standardChart, bioChart, standardDVH, lqlDVH);
                    WriteAndPlotDVH(wb, ws, colStart, bioChart, lqlDVH, structure.Id + " (LQL DVH)");

                    colStart += 2 + DataColumnSkip;
                }
            }

            // Format all numeric cells
            Range numbersRange = ws.Range[(Range)ws.Cells[DataRowNumber, 1], (Range)ws.Cells[999, 999]];
            numbersRange.NumberFormat = "0.0000";

            // Autofit columns
            Range allRange = ws.Range[(Range)ws.Cells[DataTitleRowNumber, 1], (Range)ws.Cells[999, 999]];
            allRange.Columns.AutoFit();
        }

        private static void WriteTitle(Workbook wb, Worksheet ws, PlanningItem plan)
        {
            Style titleStyle = wb.Styles[1];
            titleStyle.Interior.Color = ColorTranslator.ToOle(Color.White);
            Range titleRange = ws.Cells[TitleRowNumber, TitleColumnNumber];
            titleRange.Style = titleStyle;
            titleRange.Value = "DVHs for plan " + plan.Id;
        }

        private IEnumerable<Structure> GetSelectedStructures(PlanningItem plan)
        {
            return (from metricRow in ViewModel.MetricRows
                    orderby metricRow.StructureViewModel.Id
                    select metricRow.StructureViewModel.GetPlanStructure(plan)).Distinct();
        }

        private DVH GetStandardDVH(PlanningItem plan, Structure structure)
        {
            return GetDVH(plan, structure, CreateStandardDVHModel());
        }

        private DVH GetLQDVH(PlanningItem plan, Structure structure)
        {
            return GetDVH(plan, structure, CreateLQModel(structure));
        }

        private DVH GetLQLDVH(PlanningItem plan, Structure structure)
        {
            return GetDVH(plan, structure, CreateLQLModel(structure));
        }

        private DVHModel CreateStandardDVHModel()
        {
            // Determine the type of dose and volume based on user options
            DoseValuePresentation dp = ExportDVHDoseAbsolute ?
                DoseValuePresentation.Absolute : DoseValuePresentation.Relative;
            VolumePresentation vp = ExportDVHVolumeAbsolute ?
                VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;

            return new StandardDVHModel
            {
                DoseType = dp,
                VolumeType = vp
            };
        }

        private DVHModel CreateLQModel(Structure structure)
        {
            // Use the bio-dose metric for this structure to get the parameters to use
            MetricRowViewModel bioMetricRow =
                (from metricRow in ViewModel.MetricRows
                 where MetricRowHasStructureWithLQDose(metricRow, structure)
                 select metricRow).First();

            LQBioDoseDVHModel bioDVHModel =
                bioMetricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel as LQBioDoseDVHModel;

            // Determine the type of volume based on user options
            VolumePresentation vp = ExportDVHVolumeAbsolute ?
                VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;

            return new LQBioDoseDVHModel
            {
                VolumeType = vp,
                AlphaBeta = bioDVHModel.AlphaBeta,
            };
        }

        private DVHModel CreateLQLModel(Structure structure)
        {
            // Use the bio-dose metric for this structure to get the parameters to use
            MetricRowViewModel bioMetricRow =
                (from metricRow in ViewModel.MetricRows
                 where MetricRowHasStructureWithLQLDose(metricRow, structure)
                 select metricRow).First();

            LQLBioDoseDVHModel bioDVHModel =
                bioMetricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel as LQLBioDoseDVHModel;

            // Determine the type of volume based on user options
            VolumePresentation vp = ExportDVHVolumeAbsolute ?
                VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;

            return new LQLBioDoseDVHModel
            {
                VolumeType = vp,
                AlphaBeta = bioDVHModel.AlphaBeta,
                DT = bioDVHModel.DT,
            };
        }

        private bool StructureHasLQDose(Structure structure)
        {
            return ViewModel.MetricRows.Any(mr => MetricRowHasStructureWithLQDose(mr, structure));
        }

        private bool StructureHasLQLDose(Structure structure)
        {
            return ViewModel.MetricRows.Any(mr => MetricRowHasStructureWithLQLDose(mr, structure));
        }

        private bool MetricRowHasStructureWithLQDose(MetricRowViewModel mr, Structure structure)
        {
            return mr.StructureViewModel.Id == structure.Id &&
                mr.SelectedDVHMetricSetupViewModel != null &&
                mr.SelectedDVHMetricSetupViewModel.DVHModelViewModel is LQBioDoseDVHModelViewModel;
        }

        private bool MetricRowHasStructureWithLQLDose(MetricRowViewModel mr, Structure structure)
        {
            return mr.StructureViewModel.Id == structure.Id &&
                mr.SelectedDVHMetricSetupViewModel != null &&
                mr.SelectedDVHMetricSetupViewModel.DVHModelViewModel is LQLBioDoseDVHModelViewModel;
        }

        private Chart GetOrCreateStandardChart(Worksheet ws, Chart standardChart, DoseUnit doseUnit, VolumeUnit volumeUnit)
        {
            return standardChart != null ? standardChart : CreateAndAddChart(ws, doseUnit, volumeUnit, 300, 100, 600, 400);
        }

        private Chart GetOrCreateBioChart(Worksheet ws, Chart standardChart, Chart bioChart, DVH standardDVH, DVH bioDVH)
        {
            if (bioChart != null)
            {
                return bioChart;
            }
            else
            {
                if (bioDVH.DoseUnit == standardDVH.DoseUnit && bioDVH.VolumeUnit == standardDVH.VolumeUnit)
                {
                    return standardChart;
                }
                else
                {
                    return CreateAndAddChart(ws, bioDVH.DoseUnit, bioDVH.VolumeUnit, 300, 600, 600, 400);
                }
            }
        }

        private Chart CreateAndAddChart(Worksheet ws, DoseUnit doseUnit, VolumeUnit volumeUnit,
            double left, double top, double width, double height)
        {
            ChartObjects charts = ws.ChartObjects(Type.Missing);
            ChartObject chartObject = charts.Add(left, top, width, height);
            Chart chart = chartObject.Chart;
            chart.ChartType = XlChartType.xlXYScatterLinesNoMarkers;

            Axis xAxis = chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
            xAxis.HasTitle = true;
            xAxis.AxisTitle.Text = string.Format("Dose ({0})", DoseUnitToString(doseUnit));

            Axis yAxis = chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
            yAxis.HasTitle = true;
            yAxis.AxisTitle.Text = string.Format("Volume ({0})", VolumeUnitToString(volumeUnit));

            return chart;
        }

        private void WriteAndPlotDVH(Workbook wb, Worksheet ws, int colStart, Chart chart, DVH dvh, string seriesName)
        {
            if (dvh != null)
            {
                WriteDVH(wb, ws, colStart, dvh, seriesName);
                PlotDVH(ws, chart, dvh, seriesName);
            }
        }

        private void WriteDVH(Workbook wb, Worksheet ws, int columnStart, DVH dvh, string seriesName)
        {
            ws.Cells[StructureNameRowNumber, columnStart] = seriesName;

            // Write Dose/Volume headers (styled)
            Style headerStyle = wb.Styles[2];
            headerStyle.Font.Bold = true;
            headerStyle.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
            Range headerRange = ws.Range[(Range)ws.Cells[DataTitleRowNumber, columnStart], (Range)ws.Cells[DataTitleRowNumber, columnStart + 1]];
            headerRange.Style = headerStyle;
            string doseHeader = string.Format("Dose ({0})", DoseUnitToString(dvh.DoseUnit));
            string volumeHeader = string.Format("Volume ({0})", VolumeUnitToString(dvh.VolumeUnit));
            string[] headers = new string[2] { doseHeader, volumeHeader };
            headerRange.Value2 = headers;

            int n = dvh.CurveData.Count();
            double[,] data = new double[n, 2];

            for (int i = 0; i < n; i++)
            {
                data[i, 0] = dvh.CurveData[i].Dose;
                data[i, 1] = dvh.CurveData[i].Volume;
            }

            Range dataRange = ws.Range[ws.Cells[DataRowNumber,         columnStart],
                                       ws.Cells[DataRowNumber + n - 1, columnStart + 1]];
            dataRange.Value2 = data;
        }

        private void PlotDVH(Worksheet ws, Chart chart, DVH dvh, string seriesName)
        {
            SeriesCollection seriesCollection = chart.SeriesCollection();
            Microsoft.Office.Interop.Excel.Series series = seriesCollection.NewSeries();
            series.XValues = (from point in dvh.CurveData select point.Dose).ToArray();
            series.Values = (from point in dvh.CurveData select point.Volume).ToArray();
            series.Name = seriesName;
        }

        private DVH GetDVH(PlanningItem plan, Structure structure, DVHModel dvhModel)
        {
            EclipseData eclipseData = new EclipseData
            {
                Patient = ViewModel.Patient,
                Course = ViewModel.Course,
                Plan = plan,
                Structure = structure
            };

            try
            {
                DVH dvh = dvhModel.Calculate(eclipseData);

                if (dvh != null)
                {
                    // If user wants direct DVH, calculate it
                    return ExportDVHTypeCumulative ? dvh : ConvertToDirect(dvh);
                }
            }
            catch (Exception e)
            {
                ViewModel.NotifyUserMessaged("Error", e.Message, UserMessageType.Error);
                // Leave dvh as null; do nothing else
            }

            return null;
        }

        private string DoseUnitToString(DoseUnit doseUnit)
        {
            switch (doseUnit)
            {
                case DoseUnit.Gy:
                    return "Gy";
                case DoseUnit.cGy:
                    return "cGy";
                case DoseUnit.Percent:
                    return "%";
                default:
                    return "Unknown unit";
            }
        }

        private string VolumeUnitToString(VolumeUnit volumeUnit)
        {
            switch (volumeUnit)
            {
                case VolumeUnit.cc:
                    return "cc";
                case VolumeUnit.Percent:
                    return "%";
                default:
                    return "Unknown unit";
            }
        }

        private string CreateValidWorksheetName(string name)
        {
            // Worksheet name cannot be longer than 31 characters.

            System.Text.StringBuilder escapedString;

            if (name.Length <= 31)
            {
                escapedString = new System.Text.StringBuilder(name);
            }
            else
            {
                escapedString = new System.Text.StringBuilder(name, 0, 31, 31);
            }

            for (int i = 0; i < escapedString.Length; i++)
            {
                if (escapedString[i] == ':' ||
                escapedString[i] == '\\' ||
                escapedString[i] == '/' ||
                escapedString[i] == '?' ||
                escapedString[i] == '*' ||
                escapedString[i] == '[' ||
                escapedString[i] == ']')
                {
                    escapedString[i] = '_';
                }
            }

            return escapedString.ToString();
        }

        private DVH ConvertToDirect(DVH dvh)
        {
            return new DVH()
            {
                DVHModel = dvh.DVHModel,
                CurveData = ConvertToDirect(dvh.CurveData),
                DoseUnit = dvh.DoseUnit,
                MinDose = dvh.MinDose,
                MaxDose = dvh.MaxDose,
                MeanDose = dvh.MeanDose,
                TotalVolume = dvh.TotalVolume,
                VolumeUnit = dvh.VolumeUnit,
            };
        }

        private DVPoint[] ConvertToDirect(DVPoint[] curveData)
        {
            int n = curveData.Length;

            DVPoint[] directCurveData = new DVPoint[n];

            DVPoint lastPoint = curveData[n - 1];
            directCurveData[n - 1] =
                new DVPoint(lastPoint.Dose, lastPoint.Volume);

            for (int i = 0; i < n - 1; i++)
            {
                double deltaVolume = curveData[i].Volume - curveData[i + 1].Volume;
                directCurveData[i] =
                    new DVPoint(curveData[i].Dose, deltaVolume);
            }

            return directCurveData;
        }

        #endregion // Write DVHs
    }
}
