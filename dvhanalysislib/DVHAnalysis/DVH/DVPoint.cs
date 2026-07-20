using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVHAnalysis
{
    public struct DVPoint
    {
        public double Dose { get; set; }
        public double Volume { get; set; }

        public DVPoint(double dose, double volume) : this()
        {
            Dose = dose;
            Volume = volume;
        }
    }
}
