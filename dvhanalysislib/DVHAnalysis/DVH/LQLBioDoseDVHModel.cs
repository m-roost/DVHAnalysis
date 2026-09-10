using System.Linq;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class LQLBioDoseDVHModel : BioDoseDVHModel
    {
        public double AlphaBeta { get; set; }
        public double DT { get; set; }

        protected override DoseValue[] ConvertToBioDose(DoseValue[] dose, int nFractions)
        {
            return dose.Select(d => new DoseValue(Eqd2Lql(d.Dose, nFractions), d.Unit)).ToArray();
        }

        private double Eqd2Lql(double d, int n)
        {
            double alphaBetaInv = 1.0/AlphaBeta;
            return d * CalcRE(d / n, DT, alphaBetaInv) / CalcRE(2.0, DT, alphaBetaInv);
        }

        private double CalcRE(double dosePerFx, double DT, double alphaBetaInv)
        {
            return dosePerFx < DT
                 ? 1 + (dosePerFx)*alphaBetaInv
                 : (DT + DT*DT*alphaBetaInv + (1 + 2*DT*alphaBetaInv)*(dosePerFx - DT))/dosePerFx;
        }
    }
}
