using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.QcAnalysis
{
    public class QcResult
    {
        public PlanningItem MainPlan { get; set; }
        public int MainPlanFractions { get; set; }

        public QcTable MetricValueTable { get; set; }
        public QcTable DailyFractionTable { get; set; }
        public QcTable PercentDailyDeviationTable { get; set; }
        public QcTable AbsoluteDoseDeviationTable { get; set; }
    }
}
