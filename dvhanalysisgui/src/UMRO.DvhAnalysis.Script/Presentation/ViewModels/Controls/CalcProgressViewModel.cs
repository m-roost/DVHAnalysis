using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    public class CalcProgressViewModel : BindableBase
    {
        private ViewModel ViewModel { get; set; }

        private string _progressText;
        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                _progressText = value;
                NotifyPropertyChanged("ProgressText");
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        public ObservableCollection<string> Notifications { get; set; }

        private bool _isFinished;
        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                _isFinished = value;
                NotifyPropertyChanged("IsFinished");
            }
        }

        public bool HasNotifications
        {
            get { return Notifications.Any(); }
        }

        public event EventHandler CalculationFinished;

        public CalcProgressViewModel(ViewModel viewModel)
        {
            ViewModel = viewModel;

            ProgressText = "Calculating. Please wait.";
            Progress = 0.0;

            Notifications = new ObservableCollection<string>();

            Notifications.CollectionChanged +=
                (sender, args) => NotifyPropertyChanged("HasNotifications");

            ViewModel.CalcProgressUpdated +=
                (sender, args) => Progress = args.Progress;

            ViewModel.CalculationNotification +=
                (sender, args) => Notifications.Add(args.Message);

            ViewModel.CalculationFinished += ViewModelCalculationFinished;
        }

        private void ViewModelCalculationFinished(object sender, EventArgs e)
        {
            ProgressText = "Done.";
            IsFinished = true;
            OnCalculationFinished();
        }

        protected virtual void OnCalculationFinished()
        {
            var handler = CalculationFinished;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
