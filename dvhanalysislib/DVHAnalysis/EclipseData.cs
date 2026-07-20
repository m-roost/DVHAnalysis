using VMS.TPS.Common.Model.API;

namespace DVHAnalysis
{
    public class EclipseData
    {
        public Patient Patient { get; set; }
        public Course Course { get; set; }
        public PlanningItem Plan { get; set; }
        public Structure Structure { get; set; }
    }
}
