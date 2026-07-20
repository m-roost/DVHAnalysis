using System;
using DVHAnalysis;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.DVHModels
{
    public abstract class DVHModelViewModel : BindableBase
    {
        public DVHModel DVHModel { get; set; }

        public DoseValuePresentation DoseType
        {
            get { return DVHModel.DoseType; }
            set
            {
                DVHModel.DoseType = value;
                NotifyPropertyChanged("DoseType");
            }
        }

        public VolumePresentation VolumeType
        {
            get { return DVHModel.VolumeType; }
            set
            {
                DVHModel.VolumeType = value;
                NotifyPropertyChanged("VolumeType");
            }
        }

        public static DVHModelViewModel CreateFromDVHModel(DVHModel dvhModel)
        {
            DVHModelViewModel dvhModelViewModel = CreateDefaultFromDVHModel(dvhModel);
            dvhModelViewModel.DVHModel = dvhModel;
            return dvhModelViewModel;
        }

        public static DVHModelViewModel CreateDefaultFromDVHModel(DVHModel dvhModel)
        {
            if (dvhModel is StandardDVHModel)
            {
                return new StandardDVHModelViewModel();
            }
            else if (dvhModel is LQBioDoseDVHModel)
            {
                return new LQBioDoseDVHModelViewModel();
            }
            else if (dvhModel is LQLBioDoseDVHModel)
            {
                return new LQLBioDoseDVHModelViewModel();
            }
            else
            {
                throw new ApplicationException("Unknown DVH model type.");
            }
        }
    }
}
