using System;
using System.Windows.Input;

namespace UMRO.DvhAnalysis.Script.Presentation.Commands
{
    public class RelayCommand : ICommand
    {
        public Action<object> ExecuteDelegate { get; set; }
        public Func<object, bool> CanExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
            {
                return CanExecuteDelegate(parameter);
            }
            else
            {
                return true;
            }
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
            {
                ExecuteDelegate(parameter);
            }
        }
    }
}
