using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVHAnalysis
{
    public class NTCPMetric : Metric
    {
        public double LKBn { get; set; }
        public double LKBm { get; set; }
        public double LKBD50 { get; set; }

        public override MetricResult Calculate(DVH dvh)
        {
            return new MetricResult
            {
                DVHModel = dvh.DVHModel,
                Metric = this,
                Value = CalculateValue(dvh),
                Unit = MetricUnit.Fraction
            };
        }

        private double CalculateValue(DVH dvh)
        {
            // Use EUD to calculate NTCP
            EUDMetric eudMetric = new EUDMetric
            {
                a = 1.0 / LKBn
            };

            double EUDg = eudMetric.Calculate(dvh).Value;
            double NTCPOper = (EUDg - LKBD50) / (LKBm * LKBD50);

            // Anything over this results in an NTCP of 100%,
            // but can give the stats package an error
            if (NTCPOper > 4.89)
            {
                NTCPOper = 4.89;
            }

            var NTCP = Statistics.NormSDist(NTCPOper);
            if (NTCP < 0) NTCP = 0;
            return NTCP;
        }
    }
}
