using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UMRO.DvhAnalysis.Script.QcAnalysis
{
    public class QcTable
    {
        public QcTable()
        {
            Columns = new Collection<QcColumn>();
            Rows = new Collection<QcRow>();
        }

        public QcTable(IEnumerable<QcColumn> columns, IEnumerable<QcRow> rows) : this()
        {
            AddColumns(columns);
            AddRows(rows);
        }

        public Collection<QcColumn> Columns { get; }
        public Collection<QcRow> Rows { get; }

        public void AddColumns(IEnumerable<QcColumn> columns)
        {
            foreach (var column in columns)
            {
                Columns.Add(column);
            }
        }

        public void AddRows(IEnumerable<QcRow> rows)
        {
            foreach (var row in rows)
            {
                row.Table = this;    // The row needs to know the table it belongs to
                Rows.Add(row);
            }
        }

        public double GetValue(string planId, string structureId, string metricName)
        {
            return GetRow(structureId, metricName)[planId];
        }

        public QcRow GetRow(string structureId, string metricName)
        {
            return Rows.First(r => r.StructureId == structureId && r.MetricName == metricName);
        }
    }
}
