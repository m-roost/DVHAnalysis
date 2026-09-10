using System.Collections.ObjectModel;
using System.ComponentModel;
using OxyPlot;

namespace UMRO.Utils.DVHViewer
{
    public class DVHViewModel : INotifyPropertyChanged
    {
        private bool _isCumDVH;

        private bool _isDoseAbsolute;

        private bool _isVolumeAbsolute;

        private PlotModel _myModel;

        private ObservableCollection<DVHCurve> _dvhCurves;

        public bool IsCumDVH
        {
            get
            {
                return _isCumDVH;
            }
            set
            {
                _isCumDVH = value;
                NotifyPropertyChanged("IsCumDVH");
            }
        }

        public bool IsDifDVH
        {
            get
            {
                return !IsCumDVH;
            }
            set
            {
                _isCumDVH = !value;
                NotifyPropertyChanged("IsDifDVH");
            }
        }

        public bool IsDoseAbsolute
        {
            get
            {
                return _isDoseAbsolute;
            }
            set
            {
                _isDoseAbsolute = value;
                NotifyPropertyChanged("IsDoseAbsolute");
            }
        }

        public bool IsDoseRelative
        {
            get
            {
                return !_isDoseAbsolute;
            }
            set
            {
                _isDoseAbsolute = !value;
                NotifyPropertyChanged("IsDoseRelative");
            }
        }

        public bool IsVolumeAbsolute
        {
            get
            {
                return _isVolumeAbsolute;
            }
            set
            {
                _isVolumeAbsolute = value;
                NotifyPropertyChanged("IsVolumeAbsolute");
            }
        }

        public bool IsVolumeRelative
        {
            get
            {
                return !_isVolumeAbsolute;
            }
            set
            {
                _isVolumeAbsolute = !value;
                NotifyPropertyChanged("IsVolumeRelative");
            }
        }

        public PlotModel MyModel
        {
            get
            {
                return _myModel;
            }
            set
            {
                _myModel = value;
                NotifyPropertyChanged("MyModel");
            }
        }

        public ObservableCollection<DVHCurve> DVHCurves
        {
            get
            {
                return _dvhCurves;
            }
            set
            {
                _dvhCurves = value;
                NotifyPropertyChanged("MyModel");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DVHViewModel()
        {
            MyModel = new PlotModel();
            _dvhCurves = new ObservableCollection<DVHCurve>();
            IsCumDVH = true;
            IsDoseAbsolute = true;
            IsVolumeAbsolute = true;
            MyModel.IsLegendVisible = false;
        }

        public void NotifyPropertyChanged(string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
