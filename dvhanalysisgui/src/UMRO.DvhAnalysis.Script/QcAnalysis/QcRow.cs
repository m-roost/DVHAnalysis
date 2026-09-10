using System;
using System.Linq;

namespace UMRO.DvhAnalysis.Script.QcAnalysis
{
    public class QcRow
    {
        // Gets set by the table when this row is added to it
        public QcTable Table { get; set; }

        public string StructureId { get; set; }
        public string MetricName { get; set; }

        // There should be a value for each plan, in the same order
        public double[] Values { get; set; }

        public double this[int index]
        {
            get { return Values[index]; }
            set { Values[index] = value; }
        }

        // Provide access to values by specifying the plan ID as the index
        public double this[string planId]
        {
            get { return Values[IndexOf(planId)]; }
            set { Values[IndexOf(planId)] = value; }
        }

        private int IndexOf(string planId)
        {
            return Array.IndexOf(Table.Columns.Select(c => c.PlanId).ToArray(), planId);
        }
    }
}
