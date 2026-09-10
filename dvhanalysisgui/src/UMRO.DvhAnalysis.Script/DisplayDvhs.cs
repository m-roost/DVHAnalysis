using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using DVHAnalysis;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace UMRO.DvhAnalysis.Script
{
    class DisplayDvhs
    {
        public DisplayDvhs(ViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ViewModel ViewModel { get; }

        public DoseValuePresentation DoseType { get; set; }
        public VolumePresentation VolumeType { get; set; }

        public DisplayDvhType DvhType { get; set; }

        public IEnumerable<DisplayDvh> GetDvhs()
        {
            _dvhColors = new DvhColors();
            _dvhColorMatches = new Dictionary<string, Color>();

            return from plan in GetSelectedPlans()
                   from structure in GetSelectedPlanStructures(plan)
                   where structure != null
                   from structureDvh in GetStructureDvhs(plan, structure)
                   select structureDvh;
        }

        // Returns plans that are visible as columns
        private IEnumerable<PlanningItem> GetSelectedPlans()
        {
            return from pvm in ViewModel.PlanViewModels
                   where pvm.IsSelected
                   select pvm.Plan;
        }

        // Returns the structures chosen for metrics for the specific plan
        private IEnumerable<Structure> GetSelectedPlanStructures(PlanningItem plan)
        {
            return (from metricRow in ViewModel.MetricRows
                    orderby metricRow.StructureViewModel.Id
                    select metricRow.StructureViewModel.GetPlanStructure(plan)).Distinct();
        }

        // Returns the DVHs (standard, LQ, and/or LQL) for the structure in the specified plan
        private IEnumerable<DisplayDvh> GetStructureDvhs(PlanningItem plan, Structure structure)
        {
            var dvhs = new List<DisplayDvh>();

            // Always get the standard DVH (if successful)
            var standardDvh = GetStandardDisplayDvh(plan, structure);
            if (standardDvh != null)
            {
                dvhs.Add(standardDvh);
            }

            if (StructureHasLqDose(structure))
            {
                dvhs.Add(GetLqDisplayDvh(plan, structure));
            }

            if (StructureHasLqlDose(structure))
            {
                dvhs.Add(GetLqLDisplayDvh(plan, structure));
            }

            return dvhs;
        }

        private DisplayDvh GetStandardDisplayDvh(PlanningItem plan, Structure structure)
        {
            return GetDisplayDvh(plan, structure, GetStandardDvh(plan, structure));
        }

        private DisplayDvh GetLqDisplayDvh(PlanningItem plan, Structure structure)
        {
            return GetDisplayDvh(plan, structure, GetLqDvh(plan, structure));
        }

        private DisplayDvh GetLqLDisplayDvh(PlanningItem plan, Structure structure)
        {
            return GetDisplayDvh(plan, structure, GetLqlDvh(plan, structure));
        }

        private bool StructureHasLqDose(Structure structure)
        {
            return ViewModel.MetricRows.Any(mr =>
                MetricRowHasStructureWithBioDose<LQBioDoseDVHModel>(mr, structure));
        }

        private bool StructureHasLqlDose(Structure structure)
        {
            return ViewModel.MetricRows.Any(mr =>
                MetricRowHasStructureWithBioDose<LQLBioDoseDVHModel>(mr, structure));
        }

        private DisplayDvh GetDisplayDvh(PlanningItem plan, Structure structure, DVH dvh)
        {
            return new DisplayDvh
            {
                Name = $"{structure.Id} [{GetDisplayDvhType(dvh)}] ({plan.Id})",
                Dvh = dvh,
                Color = GetLineColor(plan, structure),
                LineType = GetLineType(dvh)
            };
        }

        private DvhLineType GetLineType(DVH dvh)
        {
            switch (GetDisplayDvhType(dvh))
            {
                case "Phys":
                    return DvhLineType.Solid;
                case "LQ":
                    return DvhLineType.Dashed;
                case "LQL":
                    return DvhLineType.Dotted;
                default:    // Should not happen
                    return DvhLineType.Solid;
            }
        }

        private Color GetLineColor(PlanningItem plan, Structure structure)
        {
            // Each plan-structure combination has its own color
            string planStructure = $"{plan.Id}-{structure.Id}";

            if (!_dvhColorMatches.ContainsKey(planStructure))
            {
                _dvhColorMatches.Add(planStructure, _dvhColors.Next());
            }

            return _dvhColorMatches[planStructure];
        }

        private string GetDisplayDvhType(DVH dvh)
        {
            if (dvh.DVHModel is StandardDVHModel)
            {
                return "Phys";
            }
            else if (dvh.DVHModel is LQBioDoseDVHModel)
            {
                return "LQ";
            }
            else if (dvh.DVHModel is LQLBioDoseDVHModel)
            {
                return "LQL";
            }

            return string.Empty;
        }

        private DVH GetStandardDvh(PlanningItem plan, Structure structure)
        {
            return GetDvh(plan, structure, CreateStandardDvhModel());
        }

        private DVH GetLqDvh(PlanningItem plan, Structure structure)
        {
            return GetDvh(plan, structure, CreateLqModel(structure));
        }

        private DVH GetLqlDvh(PlanningItem plan, Structure structure)
        {
            return GetDvh(plan, structure, CreateLqlModel(structure));
        }

        private DVH GetDvh(PlanningItem plan, Structure structure, DVHModel dvhModel)
        {
            try
            {
                DVH dvh = dvhModel.Calculate(GetEclipseData(plan, structure));

                if (dvh != null)
                {
                    return DvhType == DisplayDvhType.Cumulative ? dvh : ConvertToDirect(dvh);
                }
            }
            catch (Exception e)
            {
                var message = $"Could not get DVH for structure \"{structure.Id}\" " +
                              $"in plan \"{plan.Id}\".\n\n{e.Message}";
                ViewModel.NotifyUserMessaged("Error", message, UserMessageType.Error);
                // Leave dvh as null; do nothing else
            }

            return null;
        }

        private EclipseData GetEclipseData(PlanningItem plan, Structure structure)
        {
            Course course = plan is PlanSetup ? ((PlanSetup)plan).Course : ((PlanSum)plan).Course;
            Patient patient = course.Patient;

            return new EclipseData
            {
                Patient = patient,
                Course = course,
                Plan = plan,
                Structure = structure
            };
        }

        private DVHModel CreateStandardDvhModel()
        {
            return new StandardDVHModel
            {
                DoseType = DoseType,
                VolumeType = VolumeType
            };
        }

        private DVHModel CreateLqModel(Structure structure)
        {
            var bioDvhModel = GetFirstBioDvhModel<LQBioDoseDVHModel>(structure);
            return new LQBioDoseDVHModel
            {
                VolumeType = VolumeType,
                AlphaBeta = bioDvhModel.AlphaBeta
            };
        }

        private DVHModel CreateLqlModel(Structure structure)
        {
            var bioDvhModel = GetFirstBioDvhModel<LQLBioDoseDVHModel>(structure);
            return new LQLBioDoseDVHModel
            {
                VolumeType = VolumeType,
                AlphaBeta = bioDvhModel.AlphaBeta,
                DT = bioDvhModel.DT
            };
        }

        private T GetFirstBioDvhModel<T>(Structure structure) where T : BioDoseDVHModel
        {
            MetricRowViewModel bioMetricRow =
                (from metricRow in ViewModel.MetricRows
                 where MetricRowHasStructureWithBioDose<T>(metricRow, structure)
                 select metricRow).First();

            return bioMetricRow.SelectedDVHMetricSetupViewModel.DVHModelViewModel.DVHModel as T;
        }

        private bool MetricRowHasStructureWithBioDose<T>(MetricRowViewModel mr, Structure structure)
            where T : BioDoseDVHModel
        {
            return mr.StructureViewModel.Id == structure.Id &&
                   mr.SelectedDVHMetricSetupViewModel?.DVHModelViewModel.DVHModel is T;
        }

        private DVH ConvertToDirect(DVH dvh)
        {
            return new DVH
            {
                DVHModel = dvh.DVHModel,
                CurveData = ConvertToDirect(dvh.CurveData),
                DoseUnit = dvh.DoseUnit,
                MinDose = dvh.MinDose,
                MaxDose = dvh.MaxDose,
                MeanDose = dvh.MeanDose,
                TotalVolume = dvh.TotalVolume,
                VolumeUnit = dvh.VolumeUnit,
            };
        }

        private DVPoint[] ConvertToDirect(DVPoint[] curveData)
        {
            int n = curveData.Length;

            DVPoint[] directCurveData = new DVPoint[n];

            DVPoint lastPoint = curveData[n - 1];
            directCurveData[n - 1] =
                new DVPoint(lastPoint.Dose, lastPoint.Volume);

            for (int i = 0; i < n - 1; i++)
            {
                double deltaVolume = curveData[i].Volume - curveData[i + 1].Volume;
                directCurveData[i] =
                    new DVPoint(curveData[i].Dose, deltaVolume);
            }

            return directCurveData;
        }

        private DvhColors _dvhColors;
        private Dictionary<string, Color> _dvhColorMatches;
    }
}
