using System;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script
{
    public static class Extensions
    {
        public static IEnumerable<Structure> GetStructures(this PlanningItem planningItem)
        {
            return planningItem.GetStructureSet().Structures;
        }

        public static StructureSet GetStructureSet(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup plan)
                return plan.StructureSet;

            if (planningItem is PlanSum planSum)
                return planSum.StructureSet;

            throw new InvalidOperationException("Unknown PlanningItem type.");
        }

        public static Course GetCourse(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup plan)
                return plan.Course;

            if (planningItem is PlanSum planSum)
                return planSum.Course;

            throw new InvalidOperationException("Unknown PlanningItem type.");
        }
    }
}
