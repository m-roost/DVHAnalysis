using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for CustomMetricEditor.xaml
    /// </summary>
    public partial class MetricEditor : Window
    {
        public MetricEditorViewModel ViewModel { get; private set; }

        public MetricEditor()
        {
            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as MetricEditorViewModel;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateSortingAndGrouping();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // In WPF 4.5, ICollectionViewLiveShaping can be used
                // to automatically update the list sorting and grouping
                // (for now, listen to changes to DVHMetricSetups and update)
                case "DVHMetricSetups":
                    UpdateSortingAndGrouping();
                    break;
            }
        }

        // TODO: When MetricsViewModel is created and shared, this should go there
        private void UpdateSortingAndGrouping()
        {
            ICollectionView cvs = CollectionViewSource.GetDefaultView(ViewModel.DVHMetricSetups);
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

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Make more MVVM by creating a command,
            // but somehow need to also handle Close()

            if (ViewModel != null)
            {
                ViewModel.SaveMetrics();
            }

            MessageBox.Show("You must restart the script " +
                "before you see any changes to the metrics.",
                "Restart the script", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            Close();
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
