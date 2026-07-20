using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;

namespace UMRO.DvhAnalysis.Script
{
    public class StructureMatchViewModel : BindableBase
    {
        private string _templateStructureId;
        public string TemplateStructureId
        {
            get { return _templateStructureId; }
            set
            {
                _templateStructureId = value;
                NotifyPropertyChanged("TemplateStructureId");
            }
        }

        private string _patientStructureId;
        public string PatientStructureId
        {
            get { return _patientStructureId; }
            set
            {
                _patientStructureId = value;
                NotifyPropertyChanged("PatientStructureId");
                NotifyPropertyChanged("PatientStructureIdDisplay");
            }
        }

        public string PatientStructureIdDisplay
        {
            get { return PatientStructureId == null ? TemplateMatchingViewModel.StructureNone : PatientStructureId; }
            set { PatientStructureId = (value == TemplateMatchingViewModel.StructureNone) ? null : value; }
        }

        public StructureMatchViewModel() { }

        public StructureMatchViewModel(string templateStructureId, string patientStructureId)
        {
            TemplateStructureId = templateStructureId;
            PatientStructureId = patientStructureId;
        }
    }
}
