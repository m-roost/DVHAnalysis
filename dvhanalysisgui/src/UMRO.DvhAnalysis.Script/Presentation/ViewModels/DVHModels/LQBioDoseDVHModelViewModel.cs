using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels
{
    public class LQBioDoseDVHModelViewModel : DVHModelViewModel
    {
        public double AlphaBeta
        {
            get { return (DVHModel as LQBioDoseDVHModel).AlphaBeta; }
            set
            {
                (DVHModel as LQBioDoseDVHModel).AlphaBeta = value;
                NotifyPropertyChanged("AlphaBeta");
            }
        }
    }
}
