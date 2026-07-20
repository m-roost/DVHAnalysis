using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class VolumeRowViewModel : BindableBase
    {
        private StructureViewModel _structure;
        public StructureViewModel Structure
        {
            get { return _structure; }
            set
            {
                _structure = value;
                NotifyPropertyChanged("Structure");
            }
        }

        private ObservableCollection<double> _volumes;
        public ObservableCollection<double> Volumes
        {
            get { return _volumes; }
            set
            {
                _volumes = value;
                NotifyPropertyChanged("Volumes");
            }
        }

        public static VolumeRowViewModel CreateForPlans
            (StructureViewModel structure, IEnumerable<PlanningItem> plans)
        {
            IEnumerable<double> volumes = from plan in plans
                                          select structure.GetStructureVolume(plan);
            return new VolumeRowViewModel
                       {
                           Structure = structure,
                           Volumes = new ObservableCollection<double>(volumes)
                       };
        }
    }
}
