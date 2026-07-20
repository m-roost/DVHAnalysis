using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PngExporter = OxyPlot.Wpf.PngExporter;

namespace UMRO.DvhAnalysis.Script
{
    public class BioEvalReportCreator
    {
        private static readonly ILogger _log = AppLog.GetLogger(nameof(BioEvalReportCreator));

        public const string DefaultRequest =
            "Evaluate biocorrected plan metrics per planning directive.";

        public const string DefaultReportNotes =
            "Bioevaluation metrics were reviewed and all fall within tolerances " +
            "listed on the planning directive.";

        public const string DefaultFinalAcknowledgment =
            "Approval of this document acknowledges agreement with the work performed.";

        private static readonly Unit PageMargin = Unit.FromInch(1.0);

        private static readonly Unit HeaderTopMargin = Unit.FromInch(0.25);
        private static readonly Unit HeaderBottomMargin = Unit.FromInch(0.05);

        private static readonly Unit BorderLineWidth = Unit.FromPoint(1.0);
        private static readonly Unit BorderPadding = Unit.FromInch(0.1);

        private static readonly Unit TableLineWidth = Unit.FromPoint(1.0);
        private static readonly Unit TableThickLineWidth = Unit.FromPoint(2.0);

        private static readonly Unit TableTextDistance = Unit.FromInch(0.1);

        private static readonly Unit TableCellVerticalPadding = Unit.FromInch(0.05);
        private static readonly Unit TableLargeVerticalPadding = Unit.FromInch(0.1);

        private static readonly Unit TableRoiColumnWidth = Unit.FromInch(1.5);
        private static readonly Unit TableMetricColumnWidth = Unit.FromInch(1.25);
        private static readonly Unit TableParametersColumnWidth = Unit.FromInch(1.75);

        private static readonly int TableNumberOfNonPlanColumns = 3;

        // Patient information
        public string PatientName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string PhysicianName { get; set; } = string.Empty;

        // Physician Request
        public string RequestText { get; set; } = DefaultRequest;

        // Physics report
        public IList<string> PlanNames { get; set; } = new List<string>();
        public IList<BioEvalMetricRow> Data { get; set; } = new List<BioEvalMetricRow>();
        public IList<DisplayDvh> Dvhs { get; set; } = new List<DisplayDvh>();
        public string ReportNotes { get; set; } = DefaultReportNotes;
        public string PhysicistName { get; set; } = string.Empty;

        // Physician Final Acknowledgment
        public string FinalAcknowledgment { get; set; } = DefaultFinalAcknowledgment;

        public void CreateReport(string path)
        {
            _log.Info($"BioEval report started: patient {PatientId}, {PlanNames.Count} plan(s), " +
                      $"{Data.Count} metric row(s)");

            var report = new Document();
            DefineStyles(report);

            var reportSection = report.AddSection();
            InitSectionPageSetup(reportSection);
            FillReportSection(reportSection);

            var pdfRenderer = new PdfDocumentRenderer();
            pdfRenderer.Document = report;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(path);

            _log.Info($"BioEval report generated: {path}");
        }

        private void DefineStyles(Document doc)
        {
            var titleStyle = doc.Styles.AddStyle("Title", "Normal");
            titleStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            titleStyle.Font.Size = 14;
            titleStyle.Font.Bold = true;

            var headerStyle = doc.Styles.AddStyle("PartHeader", "Normal");
            headerStyle.ParagraphFormat.SpaceBefore = HeaderTopMargin;
            headerStyle.ParagraphFormat.SpaceAfter = HeaderBottomMargin;
            headerStyle.Font.Size = 12;
            headerStyle.Font.Bold = true;

            var contentStyle = doc.Styles.AddStyle("Content", "Normal");
            contentStyle.ParagraphFormat.Borders.Visible = true;
            contentStyle.ParagraphFormat.Borders.Width = BorderLineWidth;
            contentStyle.ParagraphFormat.Borders.Distance = BorderPadding;

            var tableHeaderStyle = doc.Styles.AddStyle("TableHeader", "Normal");
            tableHeaderStyle.Font.Bold = true;
        }

        private static void InitSectionPageSetup(Section reportSection)
        {
            Unit pageWidth, pageHeight;
            PageSetup.GetPageSize(PageFormat.Letter, out pageWidth, out pageHeight);
            reportSection.PageSetup.PageFormat = PageFormat.Letter;
            reportSection.PageSetup.PageWidth = pageWidth;
            reportSection.PageSetup.PageHeight = pageHeight;
            reportSection.PageSetup.LeftMargin = PageMargin;
            reportSection.PageSetup.TopMargin = PageMargin;
            reportSection.PageSetup.RightMargin = PageMargin;
            reportSection.PageSetup.BottomMargin = PageMargin;
        }

        private void FillReportSection(Section reportSection)
        {
            AddFooter(reportSection);
            AddTitle(reportSection);
            AddPatientInfo(reportSection);
            AddPhysicianRequest(reportSection);
            AddPhysicsReport(reportSection);
            AddFinalAcknowledgment(reportSection);
        }

        private void AddFooter(Section section)
        {
            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.AddTabStop(GetContentWidth(section.PageSetup), TabAlignment.Right);

            footer.AddText(PatientName);
            footer.AddTab();
            footer.AddText($"DOB: {DateOfBirth:d}");
            footer.AddLineBreak();
            footer.AddText($"MR#: {PatientId}");
            footer.AddTab();
            footer.AddText($"Sex: {Sex}");
        }

        private void AddTitle(Section section)
        {
            var title = section.AddParagraph("", "Title");
            title.AddText("Special Medical Physics Consult 77370: BioEval");
            title.AddLineBreak();
            title.AddText("Evaluation of Biologically Corrected Metrics");
        }

        private void AddPatientInfo(Section reportSection)
        {
            reportSection.AddParagraph("Patient Information", "PartHeader");

            var patientInfo = reportSection.AddParagraph("", "Content");
            patientInfo.AddFormattedText("Name: ", TextFormat.Bold);
            patientInfo.AddText(PatientName);
            patientInfo.AddLineBreak();
            patientInfo.AddFormattedText("ID: ", TextFormat.Bold);
            patientInfo.AddText(PatientId);
            patientInfo.AddLineBreak();
            patientInfo.AddFormattedText("DOB: ", TextFormat.Bold);
            patientInfo.AddText(DateOfBirth?.ToShortDateString() ?? string.Empty);
            patientInfo.AddLineBreak();
            patientInfo.AddFormattedText("Sex: ", TextFormat.Bold);
            patientInfo.AddText(Sex);
            patientInfo.AddLineBreak();
            patientInfo.AddFormattedText("Physician: ", TextFormat.Bold);
            patientInfo.AddText(PhysicianName);
        }

        private void AddPhysicianRequest(Section reportSection)
        {
            reportSection.AddParagraph("Physician Request", "PartHeader");

            var request = reportSection.AddParagraph("", "Content");
            request.AddText(RequestText);
            request.AddLineBreak();
            request.AddLineBreak();
            request.AddFormattedText("Evaluation ordered by: ", TextFormat.Bold);
            request.AddText(PhysicianName);
        }

        private void AddPhysicsReport(Section reportSection)
        {
            reportSection.AddParagraph("Physics Report", "PartHeader");

            // Paragraphs and tables cannot share the same border,
            // so the work-around is to create a single table that holds everything.
            var table = reportSection.AddTable();

            // Move the table a bit to the left so that the left-most column
            // appears to be the left border of the entire physics report.
            table.Rows.LeftIndent = -BorderPadding;

            // Add a fake column that acts as the space between the left border and the contents,
            // and clear its left padding so that text can be left-aligned to the table.
            var leftBorderColumn = table.AddColumn(BorderPadding);
            leftBorderColumn.LeftPadding = 0;

            // Add normal (i.e, non-border) table columns.
            var availableWidth = GetContentWidth(reportSection.PageSetup);
            AddReportTableColumns(table, availableWidth);

            // Add a fake column that acts as the space between the right border and the contents.
            table.AddColumn(BorderPadding);

            // The first row contains the paragraph before the table (spans the whole table).
            // Its padding is set to be consistent with the border spacing.
            Row topRow = table.AddRow();
            topRow.Cells[0].MergeRight = table.Columns.Count - 1;
            topRow.TopPadding = BorderPadding;
            topRow.BottomPadding = TableTextDistance;

            // The paragraph is indented to create space between the left border and the text.
            var topParagraph = topRow.Cells[0].AddParagraph();
            topParagraph.Format.LeftIndent = BorderPadding;

            int cols = TableNumberOfNonPlanColumns + PlanNames.Count;

            if (PlanNames.Count == 1)
            {
                topParagraph.AddText("A summary of the biological dose evaluation performed is below:");

                FillReportTable(table, 10, 1, 1, cols);
            }
            else
            {
                topParagraph.AddText("A summary of the biological dose evaluation performed is below "
                    + "(the table is on a separate page at the end of the document):");

                // Create a new section for the table in landscape.
                var tableSection = reportSection.Document.AddSection();
                tableSection.PageSetup = reportSection.PageSetup.Clone();
                tableSection.PageSetup.Orientation = Orientation.Landscape;

                var table2 = tableSection.AddTable();
                var availableWidth2 = GetContentWidth(tableSection.PageSetup);

                AddReportTableColumns(table2, availableWidth2);
                FillReportTable(table2, 8, 0, 0, cols);
            }

            // The last row contains the paragraph after the table (spans the whole table).
            // Its padding is changed to be consistent with the border spacing.
            Row bottomRow = table.AddRow();
            bottomRow.Cells[0].MergeRight = table.Columns.Count - 1;
            bottomRow.TopPadding = TableTextDistance;
            bottomRow.BottomPadding = BorderPadding;

            try
            {
                var plotPath = CreateDvhPlot();
                var imagePar = bottomRow.Cells[0].AddParagraph();
                imagePar.Format.Alignment = ParagraphAlignment.Center;
                imagePar.AddImage(plotPath);
            }
            catch (Exception ex)
            {
                // Skip embedding the DVH plot, but record why.
                _log.Warn("Could not embed DVH plot in BioEval report; skipping", ex);
            }

            // The paragraph is indented to create space between the left border and the text.
            var bottomParagraph = bottomRow.Cells[0].AddParagraph();
            bottomParagraph.Format.LeftIndent = BorderPadding;
            bottomParagraph.AddFormattedText("Notes: ", TextFormat.Bold);
            bottomParagraph.AddText(ReportNotes);
            bottomParagraph.AddLineBreak();
            bottomParagraph.AddLineBreak();
            bottomParagraph.AddFormattedText("Completed by: ", TextFormat.Bold);
            bottomParagraph.AddText(PhysicistName + " (" + DateTime.Now.ToShortDateString() + ")");

            // Add the border around the entire table,
            // which represents the entire physics report section
            table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count,
                Edge.Box, BorderStyle.Single, BorderLineWidth);
        }

        private void FillReportTable(Table table, Unit fontSize, int rowStart, int colStart, int cols)
        {
            table.Format.Font.Size = fontSize;

            // Increase the vertical space to every cell
            // (will be changed for certain rows later).
            table.TopPadding = TableCellVerticalPadding;
            table.BottomPadding = TableCellVerticalPadding;

            // Create the table header row.
            Row headerRow = table.AddRow();
            headerRow.Style = "TableHeader";
            headerRow.Format.Font.Size = fontSize;
            headerRow.TopPadding = TableLargeVerticalPadding;
            headerRow.BottomPadding = TableLargeVerticalPadding;

            // Add top and bottom borders to the header row.
            table.SetEdge(rowStart, colStart, cols, 1, Edge.Top,
                BorderStyle.Single, TableThickLineWidth);
            table.SetEdge(rowStart, colStart, cols, 1, Edge.Bottom,
                BorderStyle.Single, TableLineWidth);

            // Add the table header text
            headerRow.Cells[colStart].AddParagraph("ROI");
            headerRow.Cells[colStart + 1].AddParagraph("Metric");
            for (int i = 0; i < PlanNames.Count; i++)
            {
                headerRow.Cells[i + colStart + 2].AddParagraph(PlanNames[i]);
                headerRow.Cells[i + colStart + 2].Format.Alignment = ParagraphAlignment.Center;
            }
            headerRow.Cells[PlanNames.Count + colStart + 2].AddParagraph("Parameters");

            // Add the metric data to the table.
            foreach (var dataRow in Data)
            {
                Row row = table.AddRow();
                row[rowStart].AddParagraph(dataRow.Roi);
                row[rowStart + 1].AddParagraph(dataRow.Metric);
                for (int i = 0; i < dataRow.Values.Length; i++)
                {
                    row.Cells[i + rowStart + 2].AddParagraph(dataRow.Values[i]);
                }
                row.Cells[dataRow.Values.Length + rowStart + 2].AddParagraph(dataRow.Paramaters);
            }

            // Add some extra space at the bottom of the table (last row),
            // and add the bottom border.
            int lastRowIndex = table.Rows.Count - 1;
            table.Rows[lastRowIndex].BottomPadding = TableLargeVerticalPadding;
            table.SetEdge(rowStart, lastRowIndex, cols, 1, Edge.Bottom,
                BorderStyle.Single, TableThickLineWidth);
        }

        private void AddReportTableColumns(Table table, Unit availableWidth)
        {
            // Add ROI column.
            table.AddColumn(TableRoiColumnWidth);

            // Add Metric column.
            table.AddColumn(TableMetricColumnWidth);

            // Each plan column should equally take up the space left.
            var planColumnWidth = (availableWidth
                                 - TableRoiColumnWidth
                                 - TableMetricColumnWidth
                                 - TableParametersColumnWidth)
                                 / PlanNames.Count;

            // Add Plan columns.
            foreach (var plan in PlanNames)
            {
                var planColumn = table.AddColumn(planColumnWidth);
                planColumn.Format.Alignment = ParagraphAlignment.Center;
            }

            // Add Parameters column.
            table.AddColumn(TableParametersColumnWidth);
        }

        private Unit GetContentWidth(PageSetup pageSetup)
        {
            double pageWidth = pageSetup.Orientation == Orientation.Portrait
                ? pageSetup.PageWidth.Inch
                : pageSetup.PageHeight.Inch;
            double leftMargin = pageSetup.LeftMargin.Inch;
            double rightMargin = pageSetup.RightMargin.Inch;
            return Unit.FromInch(pageWidth - leftMargin - rightMargin);
        }

        private string CreateDvhPlot()
        {
            var plotModel = new PlotModel();

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MinimumPadding = 0.025,
                MaximumPadding = 0.025,
                MajorStep = 10,
                MinorStep = 5,
                MajorGridlineStyle = LineStyle.Automatic,
                Title = "Dose",
                Unit = "Gy",
                TitleFontWeight = FontWeights.Bold
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MinimumPadding = 0.025,
                MaximumPadding = 0.025,
                MajorStep = 10,
                MinorStep = 5,
                MajorGridlineStyle = LineStyle.Automatic,
                Title = "Volume",
                Unit = "%",
                TitleFontWeight = FontWeights.Bold
            };

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            foreach (var dvh in Dvhs)
            {
                plotModel.Series.Add(CreateDvhSeries(dvh));
            }

            plotModel.IsLegendVisible = true;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.BottomCenter;
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
//            plotModel.LegendOrientation = LegendOrientation.Vertical;
//            plotModel.LegendMaxHeight = 150.0;
//            plotModel.LegendColumnSpacing = 20.0;

            var tempPath = Path.GetTempFileName();
            PngExporter.Export(plotModel, tempPath, 1800, 1800, OxyColors.White, 300);
            return tempPath;
        }

        private Series CreateDvhSeries(DisplayDvh dvh)
        {
            var series = new LineSeries();
            series.Title = dvh.Name;
            series.Points.AddRange(from p in dvh.Dvh.CurveData
                                   select new DataPoint(p.Dose, p.Volume));
            series.Color = OxyColor.FromRgb(dvh.Color.R, dvh.Color.G, dvh.Color.B);
            series.LineStyle = ConvertLineStyle(dvh.LineType);
            return series;
        }

        private LineStyle ConvertLineStyle(DvhLineType lineType)
        {
            switch (lineType)
            {
                case DvhLineType.Solid:
                    return LineStyle.Solid;
                case DvhLineType.Dashed:
                    return LineStyle.Dash;
                case DvhLineType.Dotted:
                    return LineStyle.Dot;
                default:
                    return LineStyle.None;
            }
        }

        private void AddFinalAcknowledgment(Section reportSection)
        {
            reportSection.AddParagraph("Physician Final Acknowledgment", "PartHeader");
            reportSection.AddParagraph(FinalAcknowledgment, "Content");
        }
    }
}