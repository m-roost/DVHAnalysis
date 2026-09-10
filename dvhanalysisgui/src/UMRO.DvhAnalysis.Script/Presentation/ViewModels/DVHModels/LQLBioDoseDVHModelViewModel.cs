using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels
{
    public class LQLBioDoseDVHModelViewModel : DVHModelViewModel
    {
        public double AlphaBeta
        {
            get { return (DVHModel as LQLBioDoseDVHModel).AlphaBeta; }
            set
            {
                (DVHModel as LQLBioDoseDVHModel).AlphaBeta = value;
                NotifyPropertyChanged("AlphaBeta");
            }
        }

        public double DT
        {
            get { return (DVHModel as LQLBioDoseDVHModel).DT; }
            set
            {
                (DVHModel as LQLBioDoseDVHModel).DT = value;
                NotifyPropertyChanged("DT");
            }
        }
    }
}
