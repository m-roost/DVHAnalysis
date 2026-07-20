using VMS.TPS.Common.Model.API;

namespace DVHAnalysis
{
    public static class Extensions
    {
        public static Course GetCourse(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
            {
                return ((PlanSetup)planningItem).Course;
            }
            else
            {
                return ((PlanSum)planningItem).Course;
            }
        }

        public static StructureSet GetStructureSet(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
            {
                return ((PlanSetup)planningItem).StructureSet;
            }
            else
            {
                return ((PlanSum)planningItem).StructureSet;
            }
        }
    }
}
