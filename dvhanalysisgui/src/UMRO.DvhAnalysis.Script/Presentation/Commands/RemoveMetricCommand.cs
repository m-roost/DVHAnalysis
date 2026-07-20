using System;
using System.Collections.Specialized;
using System.Windows.Input;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Commands
{
    public class RemoveMetricCommand : ICommand
    {
        private MetricEditorViewModel MetricEditorViewModel { get; set; }

        public RemoveMetricCommand(MetricEditorViewModel customMetricEditor)
        {
            MetricEditorViewModel = customMetricEditor;

            // Listen to changes in custom metrics,
            // so that CanExecute is called appropriately
            MetricEditorViewModel.DVHMetricSetups.CollectionChanged += OnCustomMetricsChanged;
        }

        private void OnCustomMetricsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Notify WPF that CanExecute needs to be called
            CanExecuteChanged(sender, e);
        }

        public bool CanExecute(object parameter)
        {
            return MetricEditorViewModel.DVHMetricSetups.Count > 0;
        }

        public event EventHandler CanExecuteChanged;

        // TODO: Do this in the view model, being careful because
        // RemoveAt method messes up binding, which takes execution time,
        // even though we change the selected item right after
        public void Execute(object parameter)
        {
            int selectedIndex = GetIndexOfSelectedMetric();

            if (selectedIndex < 0)
            {
                return;
            }

            int newIndex = GetIndexAfterRemoveAt(selectedIndex);

            MetricEditorViewModel.DVHMetricSetups.RemoveAt(selectedIndex);

            MetricEditorViewModel.SelectedDVHMetricSetup = newIndex >= 0 ?
                MetricEditorViewModel.DVHMetricSetups[newIndex] : null;
        }

        private int GetIndexOfSelectedMetric()
        {
            return MetricEditorViewModel.DVHMetricSetups.IndexOf
                (MetricEditorViewModel.SelectedDVHMetricSetup);
        }

        private int GetIndexAfterRemoveAt(int index)
        {
            return IsLast(index) ? index - 1 : index;
        }

        private bool IsLast(int index)
        {
            return index == MetricEditorViewModel.DVHMetricSetups.Count - 1;
        }
    }
}
