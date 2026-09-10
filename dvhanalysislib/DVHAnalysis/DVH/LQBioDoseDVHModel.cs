using System.Linq;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class LQBioDoseDVHModel : BioDoseDVHModel
    {
        public double AlphaBeta { get; set; }

        protected override DoseValue[] ConvertToBioDose(DoseValue[] dose, int nFractions)
        {
            return dose.Select(d => new DoseValue(Eqd2(d.Dose, nFractions), d.Unit)).ToArray();
        }

        private double Eqd2(double d, int n)
        {
            return d * (d / n + AlphaBeta) / (2.0 + AlphaBeta);
        }
    }
}
