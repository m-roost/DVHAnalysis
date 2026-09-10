using System;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    public class CalculatingEventArgs : EventArgs
    {
        public CalcProgressViewModel CalcProgressViewModel { get; set; }

        public CalculatingEventArgs(CalcProgressViewModel calcProgressViewModel)
        {
            CalcProgressViewModel = calcProgressViewModel;
        }
    }
}