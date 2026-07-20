using System.Windows;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    public partial class BioEvalReportDialog : Window
    {
        public BioEvalReportDialog()
        {
            InitializeComponent();
        }

        public BioEvalReportDialogAction Action { get; private set; } =
            BioEvalReportDialogAction.None;

        public string PhysicianRequest
        {
            get { return PhysicianRequestTextBox.Text; }
            set { PhysicianRequestTextBox.Text = value; }
        }

        public string PhysicsReportNotes
        {
            get { return PhysicsReportNotesTextBox.Text; }
            set { PhysicsReportNotesTextBox.Text = value; }
        }

        public string PhysicianFinalAcknowledgment
        {
            get { return PhysicianFinalAcknowledgmentTextBox.Text; }
            set { PhysicianFinalAcknowledgmentTextBox.Text = value; }
        }

        public bool ShowToDocumentsButton
        {
            get { return SaveToDocumentsButton.Visibility == Visibility.Visible; }
            set { SaveToDocumentsButton.Visibility =
                    value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private void ShowButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Action = BioEvalReportDialogAction.Show;
            Close();
        }

        private void SaveToDocumentsButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Action = BioEvalReportDialogAction.SaveToDocuments;
            Close();
        }
    }
}
