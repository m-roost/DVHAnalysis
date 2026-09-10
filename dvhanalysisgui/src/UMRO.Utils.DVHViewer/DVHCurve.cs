using System.ComponentModel;
using System.Windows.Media;

namespace UMRO.Utils.DVHViewer
{
    public class DVHCurve : INotifyPropertyChanged
    {
        private bool _isVisible;

        public string StructureId { get; set; }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyPropertyChanged("IsVisible");
            }
        }

        public Color Color { get; set; }

        public double MaxVolume { get; set; }

        public double MaxDose { get; set; }

        public string DoseUnit { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DVHCurve()
        {
        }

        public DVHCurve(string structureId, bool isVisible, Color color, double maxDose, double maxVol, string doseUnit)
        {
            StructureId = structureId;
            IsVisible = isVisible;
            Color = color;
            MaxDose = maxDose;
            MaxVolume = maxVol;
            DoseUnit = doseUnit;
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
