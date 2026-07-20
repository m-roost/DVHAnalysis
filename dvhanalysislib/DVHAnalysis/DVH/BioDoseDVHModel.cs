using System;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public abstract class BioDoseDVHModel : DVHModel
    {
        private DoseValuePresentation _doseType = DoseValuePresentation.Absolute;

        public override DoseValuePresentation DoseType
        {
            get { return _doseType; }
        }

        protected override DVH CalculateBase(EclipseData eclipseData)
        {
            PlanningItem plan = eclipseData.Plan;
            Structure structure = eclipseData.Structure;

            var doseCloud = new StructureDoseCloud(structure, plan);

            DoseValue[] dose = GetPointBioDoses(plan, doseCloud);

            double[] doseData = dose.Select(d => d.Dose).ToArray();
            var doseUnit = dose[0].Unit;

            return new DVH
            {
                DVHModel = this,
                CurveData = GetCurveData(doseData, doseCloud.VoxelVolume),
                DoseUnit = (DoseUnit)doseUnit,
                VolumeUnit = DVHConverter.ConvertVolumeUnit(VolumeType),
                MinDose = doseData.Min(),
                MaxDose = doseData.Max(),
                MeanDose = doseData.Average(),
                MedianDose = Statistics.Median(doseData),
                StdDevDose = Statistics.StdDev(doseData),
                TotalVolume = structure.Volume
            };
        }

        private DoseValue[] GetPointBioDoses(PlanningItem plan, StructureDoseCloud doseCloud)
        {
            return plan is PlanSetup
                 ? GetPointBioDoses((PlanSetup)plan, doseCloud)
                 : GetPointBioDoses((PlanSum)plan, doseCloud);
        }

        private DoseValue[] GetPointBioDoses(PlanSetup planSetup, StructureDoseCloud doseCloud)
        {
            if (DoseIsNotAbsolute(planSetup))
            {
                throw new InvalidOperationException(
                    "The plan must be prescribed in absolute dose.");
            }

            planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            DoseValue[] pointDoses = doseCloud.GetDosesFrom(planSetup);

            if (pointDoses.Any(d => double.IsNaN(d.Dose)))    // Missing doses are specified as NaN
            {
                throw new InvalidOperationException(
                    "Some points don't have valid doses. " +
                    "Perhaps the dose grid doesn't cover the whole structure.");
            }

            // EQD2/LQL conversions are defined in Gy (the reference dose and alpha/beta
            // are in Gy), so normalize point doses to Gy first to support cGy systems.
            DoseValue[] pointDosesInGy = pointDoses.Select(d => DVHConverter.ToGy(d)).ToArray();

            return ConvertToBioDose(pointDosesInGy, GetNumberOfFractions(planSetup));
        }

        private bool DoseIsNotAbsolute(PlanSetup planSetup)
        {
            var doseUnit = planSetup.DosePerFraction.Unit;
            return !(doseUnit == DoseValue.DoseUnit.Gy || doseUnit == DoseValue.DoseUnit.cGy);
        }

        // Needs to be overridden by a subclass
        protected abstract DoseValue[] ConvertToBioDose(DoseValue[] dose, int nFractions);

        private static int GetNumberOfFractions(PlanSetup planSetup)
        {
            if (planSetup.NumberOfFractions != null)
            {
                return (int)planSetup.NumberOfFractions;
            }

            throw new InvalidOperationException(
                "Could not get the number of fractions for the plan.");
        }

        private DoseValue[] GetPointBioDoses(PlanSum planSum, StructureDoseCloud doseCloud)
        {
            DoseValue[] pointBioDoses = new DoseValue[doseCloud.Count];

            // Each component's bio-dose is normalized to Gy (EQD2) by the PlanSetup
            // overload below, so accumulate the plan-sum total in Gy as well. (Seeding
            // with the plan's native unit would mix cGy and Gy when summing.)
            var doseUnit_detected = DoseValue.DoseUnit.Gy;

            for (int i = 0; i < doseCloud.Count; i++)
            {
                pointBioDoses[i] = new DoseValue(0.0, doseUnit_detected);
            }

            foreach (var planComp in planSum.PlanSumComponents)
            {
                var plan = planSum.PlanSetups.First(p => p.Id == planComp.PlanSetupId);

                double weight = planComp.PlanWeight;
                double sign = GetPlanSign(planComp);

                var planBioDose = GetPointBioDoses(plan, doseCloud);

                for (int i = 0; i < doseCloud.Count; i++)
                {
                    pointBioDoses[i] += planBioDose[i] * weight * sign;
                }
            }

            return pointBioDoses;
        }

        private double GetPlanSign(PlanSumComponent comp)
        {
            switch (comp.PlanSumOperation)
            {
                case PlanSumOperation.Addition:
                    return 1.0;
                case PlanSumOperation.Subtraction:
                    return -1.0;
                default:
                    throw new InvalidOperationException("Cannot determine plan sum operation.");
            }
        }

        private DVPoint[] GetCurveData(double[] dose, double voxelVolume)
        {
            var dvh = new DoseVolumeHistogram(dose, voxelVolume, BinSize);
            return VolumeType == VolumePresentation.AbsoluteCm3
                 ? dvh.Curve
                 : DVHConverter.Normalized(dvh.Curve);
        }
    }
}
