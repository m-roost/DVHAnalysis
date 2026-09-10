using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UMRO.DvhAnalysis.Logging;
using UMRO.DvhAnalysis.Logging.NLog;
using UMRO.DvhAnalysis.Script.QcAnalysis;
using static UMRO.DvhAnalysis.Script.Presentation.Controls.SaveToDBWindow;

namespace UMRO.DvhAnalysis.Script
{
    internal class PDFExport_SaveToDBWindow
    {
        private static readonly ILogger _log = AppLog.GetLogger(nameof(PDFExport_SaveToDBWindow));

        private static readonly Unit HeaderTopMargin = Unit.FromInch(0.25);
        private static readonly Unit HeaderBottomMargin = Unit.FromInch(0.05);
        private static readonly Unit BorderLineWidth = Unit.FromPoint(1.0);
        private static readonly Unit BorderPadding = Unit.FromInch(0.1);
        private static readonly Unit PageMargin = Unit.FromInch(0.5); // narrow page margin

        private static readonly Unit TableCellVerticalPadding = Unit.FromInch(0.05);
        private static readonly Unit TableLargeVerticalPadding = Unit.FromInch(0.1);

        private static readonly Unit TableRoiColumnWidth = Unit.FromInch(1.25);
        private static readonly Unit TableMetricColumnWidth = Unit.FromInch(1.75);
        private static readonly Unit TableCommentColumnWidth = Unit.FromInch(1.75);

        private static readonly Unit TableLineWidth = Unit.FromPoint(1.0);
        private static readonly Unit TableThickLineWidth = Unit.FromPoint(2.0);


        public string PatientName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public PDFExport_SaveToDBWindow(string patientName, string patientId, DateTime? dateOfBirth, string sex, string physicianName)
        {
            PatientName = patientName ?? string.Empty;
            PatientId = patientId ?? string.Empty;
            DateOfBirth = dateOfBirth;
            Sex = sex ?? string.Empty;
            UserName = physicianName ?? string.Empty;
        }


        public string ExportDataGridToPdf(DataGrid dataGrid)
        {
            _log.Info($"Generating metrics PDF for patient {PatientId}");

            string reportPath = Path.GetTempFileName() + ".pdf";

            var report = new Document();
            DefineStyles(report);

            var reportSection = report.AddSection();

            InitSectionPageSetup(reportSection);

            AddFooter(reportSection);

            FillReportSection(reportSection, dataGrid);

            var pdfRenderer = new PdfDocumentRenderer();
            pdfRenderer.Document = report;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(reportPath);

            _log.Info($"Generated metrics PDF: {reportPath}");

            return reportPath;
        }

        private void AddFooter(Section section)
        {
            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.AddTabStop(GetContentWidth(section.PageSetup) / 2, TabAlignment.Right);

            footer.AddText(PatientName);
            footer.AddTab();
            footer.AddText($"MRN: {PatientId}");
            footer.AddTab();
            footer.AddText($"DOB: {DateOfBirth:d}");
            footer.AddTab();
            footer.AddText($"Sex: {Sex}");
            footer.AddLineBreak();
            footer.AddText($"Table Created by User: {UserName}");

            //footer.AddLineBreak();

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

            reportSection.PageSetup.PageFormat = PageFormat.Letter;
            
            PageSetup.GetPageSize(PageFormat.Letter, out pageWidth, out pageHeight);
            
            reportSection.PageSetup.PageWidth = pageWidth;
            reportSection.PageSetup.PageHeight = pageHeight;

            reportSection.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;

            reportSection.PageSetup.LeftMargin = PageMargin;
            reportSection.PageSetup.TopMargin = PageMargin;
            reportSection.PageSetup.RightMargin = PageMargin;
            reportSection.PageSetup.BottomMargin = PageMargin;

        }

        private void FillReportSection(Section reportSection, DataGrid dataGrid)
        {
            AddTitle(reportSection);

            AddDataGrid(reportSection, dataGrid);
        }

        private void AddTitle(Section section)
        {
            var title = section.AddParagraph("", "Title");
            title.AddText("Metrics Table Export to PDF");
            title.Format.SpaceAfter = new Unit(8, UnitType.Point);
        }

        private void AddDataGrid(Section reportSection, DataGrid dataGrid)
        {
            var table = reportSection.AddTable();

            var availableWidth = GetContentWidth(reportSection.PageSetup);

            AddDataGridColumns(table, dataGrid, availableWidth);

            FillDataGridTable(table, dataGrid);

            
            var note = reportSection.AddParagraph("* Selected cells are marked metrics used for making clinical decision.");
            note.Format.SpaceBefore = new Unit(5, UnitType.Point);
        }

        private void AddDataGridColumns(Table table, DataGrid dataGrid, Unit availableWidth)
        {
            table.AddColumn(TableRoiColumnWidth);

            table.AddColumn(TableMetricColumnWidth);

            var metric_columnWidth = (availableWidth - TableRoiColumnWidth - TableMetricColumnWidth - TableCommentColumnWidth) 
                / (dataGrid.Columns.Count - n_col_offset - 1);

            foreach (var column in dataGrid.Columns)
            {
                var dataGridColumn = table.AddColumn(metric_columnWidth);
                dataGridColumn.Format.Alignment = ParagraphAlignment.Center;

            }

            table.AddColumn(TableCommentColumnWidth);
        }

        private void FillDataGridTable(Table table, DataGrid dataGrid)
        {
            int cols = dataGrid.Columns.Count;

            var selected_cells = dataGrid.SelectedCells;

            var headerRow = table.AddRow();

            headerRow.Style = "TableHeader";

            headerRow.Height = Unit.FromPoint(40);

            headerRow.TopPadding = TableLargeVerticalPadding;
            headerRow.BottomPadding = TableLargeVerticalPadding;

            table.SetEdge(0, 0, cols, 1, Edge.Top, BorderStyle.Single, TableThickLineWidth);
            table.SetEdge(0, 0, cols, 1, Edge.Bottom, BorderStyle.Single, TableLineWidth);

            foreach (var column in dataGrid.Columns)
            {
                var hcell1 = headerRow.Cells[column.DisplayIndex];

                hcell1.AddParagraph(column.Header.ToString());

                hcell1.VerticalAlignment = VerticalAlignment.Center;

                if (column.DisplayIndex >= n_col_offset && column.DisplayIndex < dataGrid.Columns.Count - 1)
                {
                    hcell1.Format.Alignment = ParagraphAlignment.Center;

                    if (dataGrid.Columns.Count > 8)
                    {
                        if (column.DisplayIndex % 2 == 1)
                        {
                            hcell1.VerticalAlignment = VerticalAlignment.Bottom;
                        }
                        else
                        {
                            hcell1.VerticalAlignment = VerticalAlignment.Top;
                        }
                    }
                }

                if (column.DisplayIndex == dataGrid.Columns.Count - 1)
                {
                    hcell1.Format.Alignment = ParagraphAlignment.Center;
                }
            }

            foreach (var item in dataGrid.Items)
            {
                metric_row mr = item as metric_row;

                var row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                row.TopPadding = TableCellVerticalPadding;
                row.BottomPadding = TableLargeVerticalPadding;

                row.Cells[0].AddParagraph(mr.Structure);

                row.Cells[1].AddParagraph(mr.MetricName);

                for (int i = 0; i < mr.Metrics.Count; i++)
                {
                    var cell1 = row.Cells[i + n_col_offset]; 
                    
                    cell1.AddParagraph(mr.Metrics[i]);

                    if(selected_cells.Any(t => t.Item == item && (t.Column.DisplayIndex - n_col_offset) == i))
                    {
                        cell1.Shading.Color = Colors.LightGray;
                    }
                }

                row.Cells[n_col_offset + mr.Metrics.Count].AddParagraph(mr.Comment);

            }

            int lastRowIndex = table.Rows.Count - 1;

            table.Rows[lastRowIndex].BottomPadding = TableLargeVerticalPadding;

            table.SetEdge(0, lastRowIndex, cols, 1, Edge.Bottom, BorderStyle.Single, TableThickLineWidth);
        }


        private const int n_col_offset = 2; // 2 columns before the metrics start (structure, MetricName)


        private Unit GetContentWidth(PageSetup pageSetup)
        {
            double pageWidth = pageSetup.Orientation == MigraDoc.DocumentObjectModel.Orientation.Portrait
                ? pageSetup.PageWidth.Inch
                : pageSetup.PageHeight.Inch;

            double leftMargin = pageSetup.LeftMargin.Inch;

            double rightMargin = pageSetup.RightMargin.Inch;

            double remained_width = pageWidth - leftMargin - rightMargin;

            return Unit.FromInch(remained_width);
        }
    }
}
