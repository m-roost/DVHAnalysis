using System.Linq;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.Presentation.ViewModels
{
    public class StructureViewModel : BindableBase
    {
        public string Id { get; set; }

        public double GetStructureVolume(PlanningItem plan)
        {
            // Get the actual structure from plan (based on structure id)
            // since different plans can have different structure sets
            Structure planStructure = GetPlanStructure(plan);

            if (planStructure != null)
            {
                return planStructure.Volume;
            }
            else
            {
                return -1.0;
            }
        }

        public Structure GetPlanStructure(PlanningItem plan)
        {
            StructureSet structureSet = GetPlanStructureSet(plan);
            return structureSet.Structures.FirstOrDefault(s => s.Id == Id);
        }

        private StructureSet GetPlanStructureSet(PlanningItem plan)
        {
            if (plan is PlanSetup)
            {
                return (plan as PlanSetup).StructureSet;
            }
            else
            {
                return (plan as PlanSum).StructureSet;
            }
        }

        public bool IsTarget(PlanningItem plan)
        {
            Structure planStructure = GetPlanStructure(plan);

            if (planStructure != null)
            {
                return new string[] { "GTV", "CTV", "PTV" }.Contains(planStructure.DicomType);
            }
            else
            {
                return false;
            }
        }
    }
}
