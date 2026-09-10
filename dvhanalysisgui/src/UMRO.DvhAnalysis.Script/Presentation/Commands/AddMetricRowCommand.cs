using System;
using System.Windows.Input;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script.Presentation.Commands
{
    public class AddMetricRowCommand : ICommand
    {
        public ViewModel ViewModel { get; set; }

        public AddMetricRowCommand(ViewModel viewModel)
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
            StructureViewModel structure = parameter as StructureViewModel;

            if (structure != null)
            {
                ViewModel.AddMetricRowForStructure(structure);
            }
        }
    }
}
