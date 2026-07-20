using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DVHAnalysis;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls
{
    public class TemplateMatchingViewModel : BindableBase
    {
        // Displayed when no structure is selected
        public const string StructureNone = "(None)";

        private string[] _patientStructureIds;
        public string[] PatientStructureIds
        {
            get { return _patientStructureIds; }
            set
            {
                _patientStructureIds = value;
                NotifyPropertyChanged("PatientStructureIds");
                NotifyPropertyChanged("PatientStructureIdsDisplay");
            }
        }

        public string[] PatientStructureIdsDisplay
        {
            get { return Enumerable.Concat(new string[] { StructureNone }, PatientStructureIds).ToArray(); }
        }

        private Template _template;
        public Template Template
        {
            get { return _template; }
            set
            {
                _template = value;
                NotifyPropertyChanged("Template");
            }
        }

        public ObservableCollection<StructureMatchViewModel> StructureMatches { get; private set; }

        public TemplateMatchingViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PatientStructureIds":
                case "Template":
                    InitializeStructureMatches();
                    break;
            }
        }

        private void InitializeStructureMatches()
        {
            if (PatientStructureIds == null || Template == null)
            {
                return;
            }

            StructureMatches = new ObservableCollection<StructureMatchViewModel>
                (from templateStructureId in GetTemplateStructureIds()
                 let patientStructureId = FindMatchingPatientStructureId(templateStructureId)
                 select new StructureMatchViewModel(templateStructureId, patientStructureId));
            NotifyPropertyChanged("StructureMatches");    // Only place where StructureMatches is set
        }

        private IEnumerable<string> GetTemplateStructureIds()
        {
            if (Template.StructureMetrics != null)
            {
                return (from sm in Template.StructureMetrics
                        select sm.StructureId).Distinct();
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        private string FindMatchingPatientStructureId(string templateStructureId)
        {
            if (templateStructureId == null)
            {
                return null;
            }

            return FindExactMatchingPatientStructureId(templateStructureId);
        }

        private string FindExactMatchingPatientStructureId(string templateStructureId)
        {
            return PatientStructureIds.FirstOrDefault(id => id == templateStructureId);
        }
    }
}
