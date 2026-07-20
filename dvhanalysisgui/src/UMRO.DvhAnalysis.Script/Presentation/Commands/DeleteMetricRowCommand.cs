using System;
using System.Windows.Input;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Commands
{
    public class DeleteMetricRowCommand : ICommand
    {
        public ViewModel ViewModel { get; set; }

        public DeleteMetricRowCommand(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ViewModel.DeleteMetricRow();
        }
    }
}
