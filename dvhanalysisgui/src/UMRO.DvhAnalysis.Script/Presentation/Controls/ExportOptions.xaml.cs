using System.Windows;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    public partial class ExportOptions : Window
    {
        // Data binding doesn't work very well with radio buttons,
        // so control properties are accessed directly.
        // Also, each radio button needs to be individually set
        // because setting one does not automatically set the other,
        // even though they have the same GroupName (.NET bug?)

        public bool ExportDVH
        {
            get { return ExportDVHCheckBox.IsChecked == true; }
            set
            {
                ExportDVHCheckBox.IsChecked = value;
            }
        }
            
        public bool ExportDVHTypeCumulative
        {
            get { return DVHTypeCumulative.IsChecked == true; }
            set
            {
                DVHTypeCumulative.IsChecked = value;
                DVHTypeDirect.IsChecked = !value;
            }
        }

        public bool ExportDVHDoseAbsolute
        {
            get { return DVHDoseAbsolute.IsChecked == true; }
            set
            {
                DVHDoseAbsolute.IsChecked = value;
                DVHDoseRelative.IsChecked = !value;
            }
        }

        public bool ExportDVHVolumeAbsolute
        {
            get { return DVHVolumeAbsolute.IsChecked == true; }
            set
            {
                DVHVolumeAbsolute.IsChecked = value;
                DVHVolumeRelative.IsChecked = !value;
            }
        }

        public bool ExportQCAnalysis
        {
            get { return ExportQCCheckBox.IsChecked == true; }
            set
            {
                ExportQCCheckBox.IsChecked = value;
            }
        }

        public ExportOptions()
        {
            InitializeComponent();
        }

        private void Export_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
