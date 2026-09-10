using System;
using System.Collections.Generic;
using System.Linq;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels;
using UMRO.DvhAnalysis.Script.Presentation.ViewModels.Controls;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Script.QcAnalysis
{
    public class QcAnalyzer
    {
        public QcAnalyzer(ViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public QcResult Analyze()
        {
            _mainPlan = GetMainPlan();
            _mainPlanFractions = GetNumberOfFractions(_mainPlan);

            _orderedPlans = GetOrderedPlans(_mainPlan);

            var metricValueTable = GetMetricValueTable();
            var dailyFractionTable = GetDailyFractionTable(metricValueTable);
            var percentDailyDeviationTable = GetPercentDailyDeviationTable(dailyFractionTable);
            var absoluteDoseDeviationTable = GetAbsoluteDoseDeviationTable(metricValueTable);

            return new QcResult
            {
                MainPlan = _mainPlan,
                MainPlanFractions = _mainPlanFractions,
                MetricValueTable = metricValueTable,
                DailyFractionTable = dailyFractionTable,
                PercentDailyDeviationTable = percentDailyDeviationTable,
                AbsoluteDoseDeviationTable = absoluteDoseDeviationTable,
            };
        }

        private PlanningItem GetMainPlan()
        {
            try
            {
                return _viewModel.Plans.Single(PlanIsMain);
            }
            catch
            {
                throw new InvalidOperationException(
                    "A single main plan was not found. Please open only one main plan.");
            }
        }

        private bool PlanIsMain(PlanningItem plan)
        {
            return !plan.Id.StartsWith("QC");
        }

        private int GetNumberOfFractions(PlanningItem plan)
        {
            try
            {
                return ((PlanSetup)plan).NumberOfFractions.Value;
            }
            catch
            {
                throw new InvalidOperationException(
                    "Unable to obtain plan's number of fractions.");
            }
        }

        // Returns a list of plans, where the first plan is the main plan
        // and the rest are the QC plans related to the main plan
        private IEnumerable<PlanningItem> GetOrderedPlans(PlanningItem mainPlan)
        {
            var orderedPlans = new List<PlanningItem>();
            orderedPlans.Add(mainPlan);
            orderedPlans.AddRange(GetOrderedQcPlans(GetRelatedQcPlans(mainPlan)));
            return orderedPlans;
        }

        private IEnumerable<PlanningItem> GetOrderedQcPlans(IEnumerable<PlanningItem> qcPlans)
        {
            return qcPlans.OrderBy(qcPlan => GetQcNumber(qcPlan));
        }

        private int GetQcNumber(PlanningItem qcPlan)
        {
            int idLength = qcPlan.Id.Length;

            try
            {
                // QC plan IDs always end with a two-digit number
                return Convert.ToInt32($"{qcPlan.Id[idLength - 2]}{qcPlan.Id[idLength - 1]}");
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Unable to obtain the two-digit number from the plan {qcPlan.Id}.");
            }
        }

        private IEnumerable<PlanningItem> GetRelatedQcPlans(PlanningItem plan)
        {
            return _viewModel.Plans.Where(p => PlanIsQc(p) && QcPlanIsRelated(p, plan));
        }

        private bool PlanIsQc(PlanningItem plan)
        {
            return plan.Id.StartsWith("QC");
        }

        private bool QcPlanIsRelated(PlanningItem qcPlan, PlanningItem mainPlan)
        {
            return qcPlan.Id.Contains(GetPlanNumber(mainPlan));
        }

        // For a plan id of "1.1vHN", this method returns "1.1"
        // (instead of "v", it could also be other letters)
        private string GetPlanNumber(PlanningItem plan)
        {
            char firstNonNumber = plan.Id.First(c => !char.IsDigit(c) && c != '.');
            int spaceIndex = plan.Id.IndexOf(firstNonNumber);

            if (spaceIndex != -1)
            {
                return plan.Id.Substring(0, spaceIndex);
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid plan ID. An example of a valid plan ID is \"1.1vHN\".");
            }
        }

        #region Metric values

        private QcTable GetMetricValueTable()
        {
            return new QcTable(GetPlanIdColumns(), GetMetricValueRows());
        }

        private IEnumerable<QcColumn> GetPlanIdColumns()
        {
            return GetPlanIds().Select(planId => new QcColumn {PlanId = planId});
        }

        private IEnumerable<string> GetPlanIds()
        {
            return _orderedPlans.Select(p => p.Id);
        }

        private IEnumerable<QcRow> GetMetricValueRows()
        {
            return _viewModel.MetricRows.Select(mr => new QcRow
            {
                StructureId = GetStructureId(mr),
                MetricName = GetMetricName(mr),
                Values = GetMetricValues(mr).ToArray()
            });
        }

        private string GetStructureId(MetricRowViewModel metricRow)
        {
            return metricRow.StructureViewModel.Id;
        }

        private string GetMetricName(MetricRowViewModel metricRow)
        {
            return metricRow.SelectedDVHMetricSetupViewModel.Name;
        }

        private IEnumerable<double> GetMetricValues(MetricRowViewModel metricRow)
        {
            return _orderedPlans.Select(plan => GetMetricValue(plan, metricRow));
        }

        private double GetMetricValue(PlanningItem plan, MetricRowViewModel metricRow)
        {
            return metricRow.MetricResults[_viewModel.Plans.IndexOf(plan)].Value;
        }

        #endregion

        #region Daily fractions

        private QcTable GetDailyFractionTable(QcTable metricValueTable)
        {
            return new QcTable(GetPlanIdColumns(), GetDailyFractionRows(metricValueTable.Rows));
        }

        private IEnumerable<QcRow> GetDailyFractionRows(IEnumerable<QcRow> metricValueRows)
        {
            return metricValueRows.Select(row => new QcRow
            {
                StructureId = row.StructureId,
                MetricName = row.MetricName,
                Values = GetDailyFractions(row).ToArray()
            });
        }

        private IEnumerable<double> GetDailyFractions(QcRow metricValueRow)
        {
            return metricValueRow.Values.Select(x => x / _mainPlanFractions);
        }

        #endregion

        #region Percent daily deviations

        private QcTable GetPercentDailyDeviationTable(QcTable dailyFractionTable)
        {
            return new QcTable(GetPlanIdColumns(),
                GetPercentDailyDeviationRows(dailyFractionTable));
        }

        private IEnumerable<QcRow> GetPercentDailyDeviationRows(QcTable dailyFractionTable)
        {
            return dailyFractionTable.Rows.Select(row => new QcRow
            {
                StructureId = row.StructureId,
                MetricName = row.MetricName,
                Values = GetPercentDailyFractions(row,
                    dailyFractionTable.GetValue(_mainPlan.Id, row.StructureId, row.MetricName)).ToArray()
            });
        }

        private IEnumerable<double> GetPercentDailyFractions(QcRow fxRows, double mainFxs)
        {
            return fxRows.Values.Select(x => (x - mainFxs) / mainFxs);
        }

        #endregion

        #region Absolute dose deviations

        private QcTable GetAbsoluteDoseDeviationTable(QcTable metricValueTable)
        {
            return new QcTable(GetPlanIdColumns(),
                GetAbsoluteDoseDeviationRows(metricValueTable));
        }

        private IEnumerable<QcRow> GetAbsoluteDoseDeviationRows(QcTable metricValueTable)
        {
            return metricValueTable.Rows.Select(row => new QcRow
            {
                StructureId = row.StructureId,
                MetricName = row.MetricName,
                Values = GetAbsoluteDoseDeviations(row,
                    metricValueTable.GetValue(_mainPlan.Id, row.StructureId, row.MetricName)).ToArray()
            });
        }

        private IEnumerable<double> GetAbsoluteDoseDeviations(QcRow metricValueRow, double mainMetricValue)
        {
            return metricValueRow.Values.Select(x => x - mainMetricValue);
        }

        #endregion

        private readonly ViewModel _viewModel;
        private PlanningItem _mainPlan;
        private int _mainPlanFractions;
        private IEnumerable<PlanningItem> _orderedPlans;
    }
}
