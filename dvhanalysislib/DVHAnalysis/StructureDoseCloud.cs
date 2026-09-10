using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class StructureDoseCloud
    {
        public StructureDoseCloud(Structure structure, PlanningItem planningItem)
        {
            _pointCloud = new StructurePointCloud(structure, planningItem);

            // Plan sums with plans having different structure sets
            // will convert NaN doses to 0.0 in case their registration
            // causes the dose grids to significantly not overlap
            // (the client should warn whether NaNs will be converted to 0.0).
            _doConvertNaNToZero = IsPlanSumAndHasDifferentDataSets(planningItem);
        }

        public int Count => _pointCloud.Count;
        public double VoxelVolume => _pointCloud.VoxelVolume;

        public DoseValue[] GetDosesFrom(PlanSetup plan)
        {
            var doses = new List<DoseValue>();

            var segmentProfiles = _pointCloud.GetSegmentProfilesOn(plan);

            foreach (var segmentProfile in segmentProfiles)
            {
                int count = segmentProfile.Count;
                VVector start = segmentProfile[0].Position;
                VVector stop = segmentProfile[count - 1].Position;

                var doseProfile = plan.Dose.GetDoseProfile(start, stop, new double[count]);

                for (int i = 0; i < count; i++)
                {
                    if (segmentProfile[i].Value)
                    {
                        var doseValue = _doConvertNaNToZero && double.IsNaN(doseProfile[i].Value)
                            ? 0.0
                            : doseProfile[i].Value;
                        doses.Add(new DoseValue(doseValue, doseProfile.Unit));
                    }
                }
            }

            return doses.ToArray();
        }

        private bool IsPlanSumAndHasDifferentDataSets(PlanningItem planningItem)
        {
            return (planningItem is PlanSum) && HasDifferentDataSets((PlanSum)planningItem);
        }

        private bool HasDifferentDataSets(PlanSum planSum)
        {
            if (planSum.PlanSetups == null)
            {
                return false;
            }

            var plans = planSum.PlanSetups.ToArray();
            return plans.Any(p => p.StructureSet.UID != plans.First().StructureSet.UID);
        }

        private readonly StructurePointCloud _pointCloud;
        private readonly bool _doConvertNaNToZero;
    }
}
