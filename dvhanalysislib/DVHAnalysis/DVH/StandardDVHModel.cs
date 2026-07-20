using System;
using System.Linq;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class StandardDVHModel : DVHModel
    {
        protected override DVH CalculateBase(EclipseData eclipseData)
        {
            DVHData dvhData = GetDVH(eclipseData);

            if (dvhData != null)
            {
                return DVHConverter.FromVarian(dvhData, this, VolumeType);
            }
            else
            {
                throw new ApplicationException("Could not generate the DVH.");
            }
        }

        public DVHData GetDVH(EclipseData eclipseData)
        {
            return eclipseData.Plan.GetDVHCumulativeData
                (eclipseData.Structure, DoseType, VolumeType, GetBinSizeInSystemUnit(eclipseData));
        }

        // Eclipse interprets the bin size in the system's dose unit. BinSize is
        // expressed in Gy, so on a cGy system scale it (0.1 Gy -> 10 cGy) to keep the
        // same resolution; otherwise 0.1 would mean 0.1 cGy (100x too many bins).
        // Relative (%) dose presentation is unit-independent and left unchanged.
        private double GetBinSizeInSystemUnit(EclipseData eclipseData)
        {
            if (DoseType != DoseValuePresentation.Absolute)
            {
                return BinSize;
            }

            DoseValue.DoseUnit systemDoseUnit = GetAbsoluteDoseUnit(eclipseData.Plan);
        
            double BinSize_scaled = BinSize / DVHConverter.FactorToGy(systemDoseUnit);

            return BinSize_scaled;
        }

        // The system's absolute dose unit (Gy or cGy). Plan.Dose.DoseMax3D reflects
        // the current dose *presentation* (often Percent), so it can't distinguish Gy
        // from cGy; the prescription's TotalDose is always in absolute units.
        private DoseValue.DoseUnit GetAbsoluteDoseUnit(PlanningItem plan)
        {
            if (plan is PlanSetup)
            {
                return ((PlanSetup)plan).TotalDose.Unit;
            }

            return ((PlanSum)plan).PlanSetups.First().TotalDose.Unit;
        }
    }
}
