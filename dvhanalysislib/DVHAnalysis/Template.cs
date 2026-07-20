using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;

namespace DVHAnalysis
{
    using MetricResults = IEnumerable<MetricResult>;
    using Plans = IEnumerable<PlanningItem>;

    // A template is a collection of structure-metric pairs;
    // this class encapsulates a single structure-metric pair
    public class StructureMetric
    {
        public string StructureId { get; set; }
        public DVHMetricSetup DVHMetricSetup { get; set; }

        public StructureMetric() { }

        public StructureMetric(string structureId, DVHMetricSetup dvhMetricSetup)
        {
            StructureId = structureId;
            DVHMetricSetup = dvhMetricSetup;
        }
    }

    public class Template
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerUserName { get; set; }
        public bool IsGlobal { get; set; }
        public bool IgnoreCase { get; set; }

        public Collection<StructureMetric> StructureMetrics { get; set; }

        public Template()
        {
            StructureMetrics = new Collection<StructureMetric>();
        }

        public void Add(string structureId, DVHMetricSetup dvhMetricSetup)
        {
            StructureMetrics.Add(new StructureMetric(structureId, dvhMetricSetup));
        }

        public IEnumerable<DVHMetricSetup> GetDVHMetricSetupsForStructure(string structureId)
        {
            return from sm in StructureMetrics
                   where string.Equals(sm.StructureId, structureId, GetStringComparison())
                   select sm.DVHMetricSetup;
        }

        public MetricResults Apply(Patient patient, Plans plans)
        {
            return plans.SelectMany(plan => Apply(patient, plan));
        }

        public MetricResults Apply(Patient patient, PlanningItem plan)
        {
            return from sm in StructureMetrics
                   where PlanContainsContouredStructure(plan, sm.StructureId)
                   select Apply(patient, plan, sm);
        }

        private bool PlanContainsContouredStructure(PlanningItem plan, string structureId)
        {
            return GetStructureSet(plan).Structures.Any
                (s => string.Equals(s.Id, structureId, GetStringComparison())
                      && !s.IsEmpty);
        }

        private StructureSet GetStructureSet(PlanningItem plan)
        {
            if (plan is PlanSetup)
            {
                return ((PlanSetup)plan).StructureSet;
            }
            else
            {
                return ((PlanSum)plan).StructureSet;
            }
        }

        private MetricResult Apply(Patient patient, PlanningItem plan, StructureMetric sm)
        {
            EclipseData eclipseData = CreateEclipseData(patient, plan, sm.StructureId);
            return sm.DVHMetricSetup.Calculate(eclipseData);
        }

        private EclipseData CreateEclipseData
            (Patient patient, PlanningItem plan, string structureId)
        {
            return new EclipseData
            {
                Patient = patient,
                Course = GetCourse(patient, plan),
                Plan = plan,
                Structure = GetPlanStructure(plan, structureId)
            };
        }

        private Course GetCourse(Patient patient, PlanningItem plan)
        {
            if (plan is PlanSetup)
            {
                return ((PlanSetup)plan).Course;
            }
            else
            {
                // PlanSum does not have a Course property,
                // so do the search manually using reference comparison
                return (from course in patient.Courses
                        where course.PlanSums.Contains(plan)
                        select course).First();
            }
        }

        private Structure GetPlanStructure(PlanningItem plan, string structureId)
        {
            return (from structure in GetStructureSet(plan).Structures
                    where string.Equals(structure.Id, structureId, GetStringComparison())
                    select structure).First();
        }

        private StringComparison GetStringComparison()
        {
            return IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }
    }
}
